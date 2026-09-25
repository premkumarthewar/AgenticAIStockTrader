namespace StockTrader.Application.Backtesting.Dtos;

public sealed record BacktestResultDto
{
    public decimal InitialCapital { get; init; }

    public decimal FinalPortfolioValue { get; init; }

    public decimal TotalProfitLoss { get; init; }

    public decimal TotalReturnPercentage { get; init; }

    public decimal WinRate { get; init; }

    public decimal MaxDrawdown { get; init; }

    public decimal ProfitFactor { get; init; }

    public decimal CAGR { get; init; }

    /// <summary>
    /// Annualized Sharpe ratio computed from daily portfolio returns, assuming a 0%
    /// risk-free rate and 252 trading days per year.
    /// </summary>
    public decimal SharpeRatio { get; init; }

    public int TotalTrades { get; init; }

    public int WinningTrades { get; init; }

    public int LosingTrades { get; init; }

    public IReadOnlyList<SimulatedTradeDto> Trades { get; init; }
        = [];

    /// <summary>
    /// The simulated portfolio's mark-to-market value for every day in the backtest
    /// range, for charting performance over time.
    /// </summary>
    public IReadOnlyList<EquityPointDto> EquityCurve { get; init; }
        = [];
}
