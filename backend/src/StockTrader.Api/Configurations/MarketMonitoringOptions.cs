namespace StockTrader.Api.Configurations;

/// <summary>
/// Bound from the "MarketMonitoring" section of appsettings.json.
/// </summary>
public sealed class MarketMonitoringOptions
{
    public const string SectionName = "MarketMonitoring";

    /// <summary>
    /// How often the background monitor checks every watched symbol's price, in seconds.
    /// </summary>
    public int PollIntervalSeconds { get; init; } = 60;
}
