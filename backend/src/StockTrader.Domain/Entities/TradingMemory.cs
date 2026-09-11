namespace StockTrader.Domain.Entities;

public class TradingMemory
{
    public Guid Id { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }

    public string SourceAgent { get; set; } = string.Empty;
}
