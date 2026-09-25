namespace StockTrader.Application.AI.Dtos;

public sealed record WatchlistAnalysisDto
{
    public IReadOnlyList<AlertDto> Alerts { get; init; } = [];

    /// <summary>
    /// One scored recommendation per watchlist symbol (previously a single block of
    /// free-form text for the whole watchlist with no per-symbol structure or score).
    /// </summary>
    public IReadOnlyList<WatchlistRecommendationDto> Recommendations { get; init; } = [];
}
