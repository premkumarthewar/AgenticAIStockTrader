namespace StockTrader.Api.Configurations;

/// <summary>
/// OpenTelemetry export settings, bound from the "Telemetry" configuration section.
/// Lives alongside MarketMonitoringOptions here (Api/Configurations) rather than under
/// Infrastructure/Options since it's purely a hosting/observability concern, not a
/// business-facing one. Leaving OtlpEndpoint unset (the default in every environment
/// today) means traces/metrics are written to the console exporter instead - useful for
/// local development without standing up a collector.
/// </summary>
public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; init; } = "StockTrader.Api";

    /// <summary>
    /// OTLP collector endpoint (e.g. "http://localhost:4317"). Null/empty falls back to
    /// the console exporter.
    /// </summary>
    public string? OtlpEndpoint { get; init; }
}
