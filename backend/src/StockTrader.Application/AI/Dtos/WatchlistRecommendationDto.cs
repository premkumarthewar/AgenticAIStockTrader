namespace StockTrader.Application.AI.Dtos;

/// <summary>
/// A single scored recommendation for one symbol on a watchlist.
/// </summary>
public sealed record WatchlistRecommendationDto
{
    public required string Symbol { get; init; }

    public required string Action { get; init; }

    /// <summary>
    /// Deterministic 0-100 composite score, computed the same way as TradingDecisionDto.RecommendationScore.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Analyst-style rating derived from Action and Score: one of "Strong Buy", "Buy", "Weak Buy", "Hold", "Weak Sell", "Sell", "Strong Sell".
    /// </summary>
    public required string Rating { get; init; }

    public required string Summary { get; init; }
}
