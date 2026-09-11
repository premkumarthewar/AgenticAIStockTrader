namespace StockTrader.Domain.Entities;

public class MemorySummary
{
    public Guid Id { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime LastUpdatedUtc { get; set; }
}
