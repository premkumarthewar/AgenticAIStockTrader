namespace StockTrader.Application.MarketData.Dtos;

public sealed record FinancialsDto
{
    public required string Symbol { get; init; }

    public decimal? EarningsPerShare { get; init; }

    public decimal? GrossMargin { get; init; }

    public decimal? OperatingMargin { get; init; }

    public decimal? NetMargin { get; init; }

    public decimal? ReturnOnEquity { get; init; }

    public decimal? ReturnOnAssets { get; init; }

    public decimal? DebtToEquity { get; init; }
}