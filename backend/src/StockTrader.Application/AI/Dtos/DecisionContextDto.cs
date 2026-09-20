namespace StockTrader.Application.AI.Dtos;

public sealed record DecisionContextDto
{
    public required string Symbol { get; init; }

    public string MarketAnalysis { get; init; } = string.Empty;

    public string ResearchAnalysis { get; init; } = string.Empty;

    public string PortfolioAnalysis { get; init; } = string.Empty;

    public string WatchlistAnalysis { get; init; } = string.Empty;

    public string MemorySummary { get; init; } = string.Empty;

    public string RiskSummary { get; init; } = string.Empty;
}
