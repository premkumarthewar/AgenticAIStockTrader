using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Finnhub.Models;

public sealed record FinnhubNewsResponse
{
    [JsonPropertyName("headline")]
    public string? Headline { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("datetime")]
    public long Datetime { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }
}
