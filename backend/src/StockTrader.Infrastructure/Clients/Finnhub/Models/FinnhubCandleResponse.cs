using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Finnhub.Models;

public sealed record FinnhubCandleResponse
{
    [JsonPropertyName("c")]
    public List<decimal>? Close { get; init; }

    [JsonPropertyName("h")]
    public List<decimal>? High { get; init; }

    [JsonPropertyName("l")]
    public List<decimal>? Low { get; init; }

    [JsonPropertyName("o")]
    public List<decimal>? Open { get; init; }

    [JsonPropertyName("t")]
    public List<long>? Timestamps { get; init; }

    [JsonPropertyName("v")]
    public List<decimal>? Volume { get; init; }

    [JsonPropertyName("s")]
    public string? Status { get; init; }
}
