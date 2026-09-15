namespace StockTrader.Application.Backtesting.Dtos;

public sealed record SimulatedTradeDto
{
    public required string Symbol { get; init; }

    public required string Action { get; init; }

    public required DateTime ExecutedAt { get; init; }

    public decimal Price { get; init; }

    public int Quantity { get; init; }

    public decimal CashBalance { get; init; }

    public decimal PortfolioValue { get; init; }
}
