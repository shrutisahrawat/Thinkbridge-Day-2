using FluentAssertions;
using QuotesApi.Models;

namespace Quotes.Tests.Unit;

public class QuoteTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullEmptyOrWhitespaceAuthor_ThrowsInvalidOperationException(string? author)
    {
        var act = () => Quote.Create(author!, "A valid quote.");

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullEmptyOrWhitespaceText_ThrowsInvalidOperationException(string? text)
    {
        var act = () => Quote.Create("A Valid Author", text!);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_AuthorExceeding200Chars_ThrowsInvalidOperationException()
    {
        var tooLongAuthor = new string('a', 201);

        var act = () => Quote.Create(tooLongAuthor, "A valid quote.");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_AuthorExactly200Chars_DoesNotThrow()
    {
        var maxLengthAuthor = new string('a', 200);

        var act = () => Quote.Create(maxLengthAuthor, "A valid quote.");

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_TextExceeding1000Chars_ThrowsInvalidOperationException()
    {
        var tooLongText = new string('a', 1001);

        var act = () => Quote.Create("A Valid Author", tooLongText);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_TextExactly1000Chars_DoesNotThrow()
    {
        var maxLengthText = new string('a', 1000);

        var act = () => Quote.Create("A Valid Author", maxLengthText);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ValidInputWithSurroundingWhitespace_TrimsAuthorAndText()
    {
        var quote = Quote.Create("  Ada Lovelace  ", "  The engine can do whatever we know how to order it to perform.  ");

        quote.Author.Should().Be("Ada Lovelace");
        quote.Text.Should().Be("The engine can do whatever we know how to order it to perform.");
    }

    [Fact]
    public void Create_ValidInput_SetsIsDeletedFalse()
    {
        var quote = Quote.Create("Ada Lovelace", "A valid quote.");

        quote.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ValidInput_SetsCreatedAtCloseToUtcNow()
    {
        var beforeCreate = DateTime.UtcNow;

        var quote = Quote.Create("Ada Lovelace", "A valid quote.");

        quote.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SoftDelete_OnActiveQuote_SetsIsDeletedTrue()
    {
        var quote = Quote.Create("Ada Lovelace", "A valid quote.");

        quote.SoftDelete();

        quote.IsDeleted.Should().BeTrue();
    }
}
