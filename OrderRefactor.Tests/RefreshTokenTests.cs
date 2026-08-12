using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderRefactor.Data;
using Xunit;

namespace OrderRefactor.Tests;

public class RefreshTokenTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DbConnection _connection;

    public RefreshTokenTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OrdersDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<OrdersDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });
        });
    }

    /// <summary>
    /// Happy path: logging in returns an access token and a refresh token.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenPair()
    {
        var client = _factory.CreateClient();

        var tokens = await LoginAsync(client);

        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));
    }

    /// <summary>
    /// Rotation: refreshing returns a NEW refresh token, not the same one back.
    /// </summary>
    [Fact]
    public async Task Refresh_WithValidToken_RotatesToNewRefreshToken()
    {
        var client = _factory.CreateClient();

        var original = await LoginAsync(client);
        var rotated = await RefreshAsync(client, original.RefreshToken);

        Assert.NotEqual(original.RefreshToken, rotated.RefreshToken);
    }

    /// <summary>
    /// Reuse detection: replaying an already-rotated refresh token is rejected,
    /// and the whole token family is revoked so the newest token stops working too.
    /// </summary>
    [Fact]
    public async Task Refresh_WithReplayedToken_RevokesEntireChain()
    {
        var client = _factory.CreateClient();

        // 1. Log in, then rotate once. The first refresh token is now spent.
        var original = await LoginAsync(client);
        var rotated = await RefreshAsync(client, original.RefreshToken);

        // 2. Replay the spent token, as a thief with a leaked token would.
        var replayResponse = await PostRefreshAsync(client, original.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        // 3. The legitimate user's current token must also be dead now:
        //    detection revokes the entire family and forces re-authentication.
        var afterBreachResponse = await PostRefreshAsync(client, rotated.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, afterBreachResponse.StatusCode);
    }

    /// <summary>
    /// A refresh token that was never issued is rejected.
    /// </summary>
    [Fact]
    public async Task Refresh_WithUnknownToken_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await PostRefreshAsync(client, "this-token-was-never-issued");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// After logout the refresh token is revoked and can no longer be exchanged.
    /// </summary>
    [Fact]
    public async Task Refresh_AfterLogout_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        var tokens = await LoginAsync(client);

        var logoutResponse = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new { refreshToken = tokens.RefreshToken });
        logoutResponse.EnsureSuccessStatusCode();

        var response = await PostRefreshAsync(client, tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ============= Helper Methods =============

    private static async Task<TokenPair> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "admin@quotes.com", password = "SecurePassword123" });

        response.EnsureSuccessStatusCode();

        var pair = await response.Content.ReadFromJsonAsync<TokenPair>();
        Assert.NotNull(pair);
        return pair!;
    }

    private static async Task<TokenPair> RefreshAsync(HttpClient client, string refreshToken)
    {
        var response = await PostRefreshAsync(client, refreshToken);
        response.EnsureSuccessStatusCode();

        var pair = await response.Content.ReadFromJsonAsync<TokenPair>();
        Assert.NotNull(pair);
        return pair!;
    }

    private static Task<HttpResponseMessage> PostRefreshAsync(HttpClient client, string refreshToken)
        => client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

    // The API returns snake_case keys (access_token, refresh_token, expires_in),
    // so each property is mapped explicitly to its JSON name.
    private sealed record TokenPair(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    public void Dispose()
    {
        _connection.Dispose();
    }
}