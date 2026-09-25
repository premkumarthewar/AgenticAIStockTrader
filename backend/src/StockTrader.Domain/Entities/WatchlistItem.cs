namespace StockTrader.Domain.Entities;

/// <summary>
/// A symbol the user has asked to be watched continuously by the background market monitor, independent of anything they currently hold in the paper portfolio.
/// </summary>
public class WatchlistItem
{
    public Guid Id { get; set; }

    public required string Symbol { get; set; }

    public DateTime AddedOnUtc { get; set; }
}
