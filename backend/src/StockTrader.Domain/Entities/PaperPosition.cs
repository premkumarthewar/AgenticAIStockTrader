namespace StockTrader.Domain.Entities;

public sealed class PaperPosition
{
    public Guid Id { get; set; }

    public Guid PaperPortfolioId { get; set; }

    public required string Symbol { get; set; }

    public decimal Quantity { get; set; }

    public decimal AveragePrice { get; set; }

    public DateTime LastUpdatedOnUtc { get; set; }

    public PaperPortfolio PaperPortfolio { get; set; } = null!;
}