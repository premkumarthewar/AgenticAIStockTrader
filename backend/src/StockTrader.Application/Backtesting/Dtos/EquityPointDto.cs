namespace StockTrader.Application.Backtesting.Dtos;

/// <summary>
/// The simulated portfolio's mark-to-market value on a single day of a backtest,
/// used to draw the equity curve and to compute drawdown/Sharpe ratio precisely
/// instead of only at trade events.
/// </summary>
public sealed record EquityPointDto
{
    public required DateTime Date { get; init; }

    public decimal PortfolioValue { get; init; }
}
