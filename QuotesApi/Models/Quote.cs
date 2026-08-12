using QuotesApi.Services;

namespace QuotesApi.Models;

public class Quote
{
    public int Id { get; private set; }
    public string Author { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    private Quote() { } // Required for EF Core

    public static Quote Create(string author, string text)
    {
        if (string.IsNullOrWhiteSpace(author) || author.Trim().Length > 200)
            throw new InvalidOperationException("Author must be between 1 and 200 characters.");
        
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 1000)
            throw new InvalidOperationException("Quote text must be between 1 and 1000 characters.");

        return new Quote
        {
            Author = author.Trim(),
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    // Testable overload: takes the timestamp from an injected clock instead of
    // reading the system clock, so tests can assert an exact CreatedAt value.
    public static Quote Create(string author, string text, IClock clock)
    {
        var quote = Create(author, text);
        quote.CreatedAt = clock.UtcNow.UtcDateTime;
        return quote;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}