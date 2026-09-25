namespace StockTrader.Application.Backtesting.Dtos;

public sealed record BacktestRequestDto
{
    public required string Symbol { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public decimal InitialCapital { get; init; }
        = 10000m;

    /// <summary>
    /// Flat commission charged per executed trade (buy or sell). Defaults to 0, which
    /// reproduces the previous zero-cost behavior.
    /// </summary>
    public decimal CommissionPerTrade { get; init; }
        = 0m;
}
