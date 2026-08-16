using System.Text.Json.Serialization;

namespace StockTrader.Application.MarketData.Dtos;

public sealed record StockQuoteDto
{
    public required string Symbol { get; init; }
    
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
    public DateTime Timestamp { get; init; }
}
