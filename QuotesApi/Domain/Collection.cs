namespace QuotesApi.Domain;

public class Collection
{
    private readonly List<CollectionItem> _items = new();

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public IReadOnlyCollection<CollectionItem> Items => _items.AsReadOnly();

    public Collection(string name, string ownerId)
    {
        SetName(name);
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new InvalidOperationException("Owner ID cannot be empty.");
        OwnerId = ownerId;
    }

    private Collection() { }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3 || name.Trim().Length > 80)
            throw new InvalidOperationException("Collection name must be between 3 and 80 characters.");
        Name = name.Trim();
    }

    public void AddItem(int quoteId)
    {
        if (_items.Count >= 50)
            throw new InvalidOperationException("Collection cannot contain more than 50 quotes.");
        if (_items.Any(x => x.QuoteId == quoteId))
            throw new InvalidOperationException($"Quote with ID {quoteId} already exists in this collection.");
            
        _items.Add(new CollectionItem(quoteId));
    }

    public void RemoveItem(int quoteId)
    {
        var existing = _items.FirstOrDefault(x => x.QuoteId == quoteId);
        if (existing == null)
            throw new InvalidOperationException($"Quote with ID {quoteId} was not found.");
            
        _items.Remove(existing);
    }
}