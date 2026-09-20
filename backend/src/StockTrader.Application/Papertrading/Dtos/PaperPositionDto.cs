namespace StockTrader.Application.PaperTrading.Dtos;

public sealed record PaperPositionDto
{
    public required string Symbol { get; init; }

    public int Quantity { get; init; }

    public decimal AveragePrice { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal MarketValue { get; init; }

    public decimal UnrealizedProfitLoss { get; init; }

    public decimal UnrealizedReturnPercentage { get; init; }
}