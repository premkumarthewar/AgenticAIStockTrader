using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Kite.Models;

public sealed record KiteMarginsData
{
    [JsonPropertyName("equity")]
    public KiteMarginSegment? Equity { get; init; }
}

public sealed record KiteMarginSegment
{
    [JsonPropertyName("net")]
    public decimal Net { get; init; }

    [JsonPropertyName("available")]
    public KiteMarginAvailable? Available { get; init; }
}

public sealed record KiteMarginAvailable
{
    [JsonPropertyName("cash")]
    public decimal Cash { get; init; }

    [JsonPropertyName("live_balance")]
    public decimal LiveBalance { get; init; }
}
