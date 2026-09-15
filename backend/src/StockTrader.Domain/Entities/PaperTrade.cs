namespace StockTrader.Domain.Entities;

public sealed class PaperTrade
{
    public Guid Id { get; set; }

    public Guid PaperPortfolioId { get; set; }

    public required string Symbol { get; set; }

    public required string Action { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalValue { get; set; }

    public DateTime ExecutedAtUtc { get; set; }

    public PaperPortfolio PaperPortfolio { get; set; } = null!;
}