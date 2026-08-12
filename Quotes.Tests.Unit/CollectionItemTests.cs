using FluentAssertions;
using QuotesApi.Domain;

namespace Quotes.Tests.Unit;

public class CollectionItemTests
{
    [Fact]
    public void Constructor_PositiveQuoteId_SetsQuoteId()
    {
        var item = new CollectionItem(42);

        item.QuoteId.Should().Be(42);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ZeroOrNegativeQuoteId_ThrowsArgumentException(int quoteId)
    {
        var act = () => new CollectionItem(quoteId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidQuoteId_SetsAddedAtCloseToUtcNow()
    {
        var beforeCreate = DateTime.UtcNow;

        var item = new CollectionItem(42);

        item.AddedAt.Should().BeOnOrAfter(beforeCreate).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
