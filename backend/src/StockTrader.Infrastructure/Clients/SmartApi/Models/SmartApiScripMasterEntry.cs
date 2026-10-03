using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.SmartApi.Models;

/// <summary>
/// One row of Angel One's published instrument/scrip master
/// (AngelOneOptions.ScripMasterUrl) - a several-MB JSON array covering every tradable
/// instrument across every exchange and segment. AngelOneInstrumentLookup downloads and
/// caches this rather than fetching it per order.
/// </summary>
public sealed record SmartApiScripMasterEntry
{
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("instrumenttype")]
    public string? InstrumentType { get; init; }

    [JsonPropertyName("exch_seg")]
    public string? ExchangeSegment { get; init; }

    [JsonPropertyName("lotsize")]
    public string? LotSize { get; init; }
}
