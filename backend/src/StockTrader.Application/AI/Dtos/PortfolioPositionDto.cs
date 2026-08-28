namespace StockTrader.Application.AI.Dtos;

public sealed record PortfolioPositionDto
{
    public required string Symbol { get; init; }

    public int Quantity { get; init; }

    public decimal AverageCost { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal MarketValue { get; init; }

    public decimal CostBasis { get; init; }

    public decimal UnrealizedPnL { get; init; }

    public decimal WeightPercentage { get; init; }
}
