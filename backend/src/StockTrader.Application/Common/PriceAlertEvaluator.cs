namespace StockTrader.Application.Common;

/// <summary>
/// Single source of truth for what counts as a price alert, shared by WatchlistAgent (on-demand watchlist analysis) and MarketMonitoringBackgroundService (continuous background monitoring), so both agree on the same thresholds and wording.
/// </summary>
public static class PriceAlertEvaluator
{
    public const decimal SurgeThresholdPercent = 5m;

    public const string PriceSurgeAlertType = "PRICE_SURGE";

    public const string PriceDropAlertType = "PRICE_DROP";

    /// <summary>
    /// Returns the alert type triggered by this percentage change, or null when the
    /// change isn't large enough to trigger an alert.
    /// </summary>
    public static string? Evaluate(decimal percentChange)
    {
        if (percentChange >= SurgeThresholdPercent)
            return PriceSurgeAlertType;

        if (percentChange <= -SurgeThresholdPercent)
            return PriceDropAlertType;

        return null;
    }

    public static string BuildMessage(string alertType, decimal percentChange)
    {
        return alertType == PriceSurgeAlertType
            ? $"Price increased by {percentChange:F2}%"
            : $"Price decreased by {percentChange:F2}%";
    }
}
