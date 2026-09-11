namespace StockTrader.AI.Memory;

public sealed record MemoryRecord
{
    public Guid Id { get; init; }

    public string Category { get; init; } = string.Empty;

    public string Symbol { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime CreatedOnUtc { get; init; } = DateTime.UtcNow;
}
