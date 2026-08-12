using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using QuotesApi.Models;

namespace Quotes.Tests.Unit;

public class CreateQuoteRequestValidationTests
{
    private static IList<ValidationResult> Validate(CreateQuoteRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsNoValidationErrors()
    {
        var request = new CreateQuoteRequest { Author = "Ada Lovelace", Text = "A valid quote." };

        var results = Validate(request);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingOrEmptyAuthor_ReturnsValidationError(string? author)
    {
        var request = new CreateQuoteRequest { Author = author!, Text = "A valid quote." };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateQuoteRequest.Author)));
    }

    [Fact]
    public void Validate_AuthorExceeding200Chars_ReturnsValidationError()
    {
        var request = new CreateQuoteRequest { Author = new string('a', 201), Text = "A valid quote." };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateQuoteRequest.Author)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingOrEmptyText_ReturnsValidationError(string? text)
    {
        var request = new CreateQuoteRequest { Author = "Ada Lovelace", Text = text! };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateQuoteRequest.Text)));
    }

    [Fact]
    public void Validate_TextExceeding1000Chars_ReturnsValidationError()
    {
        var request = new CreateQuoteRequest { Author = "Ada Lovelace", Text = new string('a', 1001) };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateQuoteRequest.Text)));
    }
}
