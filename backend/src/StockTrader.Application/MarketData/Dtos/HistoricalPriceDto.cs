namespace StockTrader.Application.MarketData.Dtos;

public sealed record HistoricalPriceDto
{
    public required string Symbol { get; init; }

    public DateTime Date { get; init; }

    public decimal Open { get; init; }

    public decimal High { get; init; }

    public decimal Low { get; init; }

    public decimal Close { get; init; }

    public decimal Volume { get; init; }
}
