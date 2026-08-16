namespace StockTrader.Contracts.AI;

/// <summary>
/// Represents the result of an AI-powered market analysis.
/// </summary>
public sealed record MarketAnalysisResult
{
    /// <summary>
    /// Gets the stock ticker symbol.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Gets the AI-generated market analysis.
    /// </summary>
    public required string Analysis { get; init; }

    /// <summary>
    /// Gets the current market price when available.
    /// </summary>
    public decimal? CurrentPrice { get; init; }

    /// <summary>
    /// Gets the detected market trend.
    /// </summary>
    public string? Trend { get; init; }

    /// <summary>
    /// Gets the confidence level of the analysis.
    /// </summary>
    public string? Confidence { get; init; }
}