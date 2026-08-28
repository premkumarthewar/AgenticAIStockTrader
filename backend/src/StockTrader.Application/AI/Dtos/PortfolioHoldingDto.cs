namespace StockTrader.Application.AI.Dtos;

public sealed record PortfolioHoldingDto
{
    public required string Symbol { get; init; }

    public required int Quantity { get; init; }

    public required decimal AverageCost { get; init; }
}
