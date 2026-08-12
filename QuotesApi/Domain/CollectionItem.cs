namespace QuotesApi.Domain;

public record CollectionItem
{
    public int QuoteId { get; init; }
    public DateTime AddedAt { get; init; }

    public CollectionItem(int quoteId)
    {
        if (quoteId <= 0)
            throw new ArgumentException("Quote ID must be a positive integer.", nameof(quoteId));

        QuoteId = quoteId;
        AddedAt = DateTime.UtcNow;
    }

    private CollectionItem() { }
}