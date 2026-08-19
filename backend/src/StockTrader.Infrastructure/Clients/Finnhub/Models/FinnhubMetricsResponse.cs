using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Finnhub.Models;

public sealed record FinnhubMetricsResponse
{
    [JsonPropertyName("metric")]
    public FinnhubMetric? Metric { get; init; }
}

public sealed record FinnhubMetric
{
    [JsonPropertyName("epsAnnual")]
    public decimal? EpsAnnual { get; init; }

    [JsonPropertyName("grossMarginAnnual")]
    public decimal? GrossMarginAnnual { get; init; }

    [JsonPropertyName("operatingMarginAnnual")]
    public decimal? OperatingMarginAnnual { get; init; }

    [JsonPropertyName("netProfitMarginAnnual")]
    public decimal? NetProfitMarginAnnual { get; init; }

    [JsonPropertyName("roeAnnual")]
    public decimal? RoeAnnual { get; init; }

    [JsonPropertyName("roaAnnual")]
    public decimal? RoaAnnual { get; init; }

    [JsonPropertyName("totalDebtToEquityAnnual")]
    public decimal? TotalDebtToEquityAnnual { get; init; }
}
