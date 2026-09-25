using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Finnhub.Models;

public sealed record FinnhubQuoteResponse
{
    [JsonPropertyName("c")]
    public decimal CurrentPrice { get; init; }

    [JsonPropertyName("d")]
    public decimal Change { get; init; }

    [JsonPropertyName("dp")]
    public decimal PercentChange { get; init; }

    [JsonPropertyName("h")]
    public decimal High { get; init; }

    [JsonPropertyName("l")]
    public decimal Low { get; init; }

    [JsonPropertyName("o")]
    public decimal Open { get; init; }

    [JsonPropertyName("pc")]
    public decimal PreviousClose { get; init; }

    [JsonPropertyName("t")]
    public long Timestamp { get; init; }
}
