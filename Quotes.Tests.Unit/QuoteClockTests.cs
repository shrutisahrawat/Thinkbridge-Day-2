using FluentAssertions;
using NSubstitute;
using QuotesApi.Models;
using QuotesApi.Services;

namespace Quotes.Tests.Unit;

public class QuoteClockTests
{
    [Fact]
    public void Create_WithFakeClock_UsesClockTimeForCreatedAt()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2020, 1, 1, 12, 0, 0, TimeSpan.Zero));

        var quote = Quote.Create("Ada Lovelace", "A valid quote.", clock);

        quote.CreatedAt.Should().Be(new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Create_WithFakeClock_IgnoresSystemTime()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.Zero));

        var quote = Quote.Create("Ada Lovelace", "A valid quote.", clock);

        quote.CreatedAt.Year.Should().Be(1999);
    }

    [Fact]
    public void Create_WithFakeClock_StillValidatesAuthor()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        var act = () => Quote.Create("", "A valid quote.", clock);

        act.Should().Throw<InvalidOperationException>();
    }
}