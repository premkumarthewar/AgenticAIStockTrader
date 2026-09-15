namespace StockTrader.Domain.Entities;

public sealed class PaperPortfolio
{
    public Guid Id { get; set; }

    public decimal InitialCapital { get; set; }

    public decimal CashBalance { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime LastUpdatedOnUtc { get; set; }

    public ICollection<PaperPosition> Positions { get; set; } = [];

    public ICollection<PaperTrade> Trades { get; set; } = [];
}