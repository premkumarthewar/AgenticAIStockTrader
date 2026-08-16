namespace StockTrader.Infrastructure.Clients.Finnhub.Models;

public sealed record FinnhubQuoteResponse
{
    public decimal CurrentPrice { get; init; }

    public decimal Change { get; init; }

    public decimal PercentChange { get; init; }

    public decimal High { get; init; }

    public decimal Low { get; init; }

    public decimal Open { get; init; }

    public decimal PreviousClose { get; init; }

    public long Timestamp { get; init; }
}
