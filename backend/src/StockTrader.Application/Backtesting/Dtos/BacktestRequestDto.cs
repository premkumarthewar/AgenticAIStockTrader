namespace StockTrader.Application.Backtesting.Dtos;

public sealed record BacktestRequestDto
{
    public required string Symbol { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public decimal InitialCapital { get; init; }
        = 10000m;
}
