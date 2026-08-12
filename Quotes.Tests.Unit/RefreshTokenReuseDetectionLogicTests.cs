using FluentAssertions;
using OrderRefactor.Models;

namespace Quotes.Tests.Unit;

// NOTE ON SCOPE: AuthController.Refresh (OrderRefactor/Controllers/AuthController.cs, ~lines 57-124) queries
// OrdersDbContext directly and has no extracted, substitutable service, so its branch logic cannot be invoked
// as a pure unit test (no DbContext/HTTP/I-O). SimulateRefresh below is a re-implementation of that same
// branch logic (token-not-found / reuse-detected-and-family-revoked / expired / rotate) running purely
// against an in-memory list. These tests validate that re-implementation, NOT the controller itself, so they
// will not catch a regression if AuthController.Refresh changes without this mirror being updated too.
public class RefreshTokenReuseDetectionLogicTests
{
    private enum RefreshOutcome
    {
        InvalidToken,
        ReuseDetected,
        Expired,
        Rotated
    }

    private static RefreshOutcome SimulateRefresh(List<RefreshToken> store, string incomingTokenHash, DateTime now)
    {
        var existingToken = store.FirstOrDefault(t => t.TokenHash == incomingTokenHash);

        if (existingToken == null)
        {
            return RefreshOutcome.InvalidToken;
        }

        if (existingToken.RevokedAt != null)
        {
            foreach (var token in store.Where(t => t.UserId == existingToken.UserId && t.RevokedAt == null))
            {
                token.RevokedAt = now;
            }

            return RefreshOutcome.ReuseDetected;
        }

        if (existingToken.ExpiresAt < now)
        {
            return RefreshOutcome.Expired;
        }

        var newToken = new RefreshToken
        {
            TokenHash = "new-hash",
            UserId = existingToken.UserId,
            ExpiresAt = now.AddDays(7)
        };
        existingToken.RevokedAt = now;
        existingToken.ReplacedByToken = newToken.TokenHash;
        store.Add(newToken);

        return RefreshOutcome.Rotated;
    }

    [Fact]
    public void SimulateRefresh_UnknownTokenHash_ReturnsInvalidToken()
    {
        var store = new List<RefreshToken>();

        var outcome = SimulateRefresh(store, "never-issued-hash", DateTime.UtcNow);

        outcome.Should().Be(RefreshOutcome.InvalidToken);
    }

    [Fact]
    public void SimulateRefresh_AlreadyRevokedToken_ReturnsReuseDetected()
    {
        var now = DateTime.UtcNow;
        var store = new List<RefreshToken>
        {
            new() { TokenHash = "spent-hash", UserId = "user-1", ExpiresAt = now.AddDays(1), RevokedAt = now.AddMinutes(-1) }
        };

        var outcome = SimulateRefresh(store, "spent-hash", now);

        outcome.Should().Be(RefreshOutcome.ReuseDetected);
    }

    [Fact]
    public void SimulateRefresh_AlreadyRevokedToken_RevokesEntireUserFamily()
    {
        var now = DateTime.UtcNow;
        var currentValidToken = new RefreshToken { TokenHash = "current-hash", UserId = "user-1", ExpiresAt = now.AddDays(1), RevokedAt = null };
        var spentToken = new RefreshToken { TokenHash = "spent-hash", UserId = "user-1", ExpiresAt = now.AddDays(1), RevokedAt = now.AddMinutes(-1) };
        var store = new List<RefreshToken> { currentValidToken, spentToken };

        SimulateRefresh(store, "spent-hash", now);

        currentValidToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void SimulateRefresh_ExpiredToken_ReturnsExpired()
    {
        var now = DateTime.UtcNow;
        var store = new List<RefreshToken>
        {
            new() { TokenHash = "expired-hash", UserId = "user-1", ExpiresAt = now.AddDays(-1), RevokedAt = null }
        };

        var outcome = SimulateRefresh(store, "expired-hash", now);

        outcome.Should().Be(RefreshOutcome.Expired);
    }

    [Fact]
    public void SimulateRefresh_ValidUnexpiredToken_ReturnsRotated()
    {
        var now = DateTime.UtcNow;
        var store = new List<RefreshToken>
        {
            new() { TokenHash = "valid-hash", UserId = "user-1", ExpiresAt = now.AddDays(1), RevokedAt = null }
        };

        var outcome = SimulateRefresh(store, "valid-hash", now);

        outcome.Should().Be(RefreshOutcome.Rotated);
    }

    [Fact]
    public void SimulateRefresh_ValidUnexpiredToken_RevokesOldTokenAndAddsReplacement()
    {
        var now = DateTime.UtcNow;
        var originalToken = new RefreshToken { TokenHash = "valid-hash", UserId = "user-1", ExpiresAt = now.AddDays(1), RevokedAt = null };
        var store = new List<RefreshToken> { originalToken };

        SimulateRefresh(store, "valid-hash", now);

        originalToken.RevokedAt.Should().NotBeNull();
        originalToken.ReplacedByToken.Should().NotBeNullOrEmpty();
        store.Should().HaveCount(2);
    }
}
