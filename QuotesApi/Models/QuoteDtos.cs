using System.ComponentModel.DataAnnotations;

namespace QuotesApi.Models;

public class CreateQuoteRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
}

public record QuoteResponse(int Id, string Author, string Text, DateTime CreatedAt)
{
    public static QuoteResponse FromEntity(Quote q) => new(q.Id, q.Author, q.Text, q.CreatedAt);
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int TotalCount);