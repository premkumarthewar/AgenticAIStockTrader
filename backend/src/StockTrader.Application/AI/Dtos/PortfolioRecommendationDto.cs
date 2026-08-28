namespace StockTrader.Application.AI.Dtos;

public sealed record PortfolioRecommendationDto
{
    public required string Recommendation { get; init; }

    public required string Reasoning { get; init; }

    public decimal? SuggestedAllocation { get; init; }
    
    public decimal PortfolioValue { get; init; }

    public decimal CashPercentage { get; init; }

    public IReadOnlyList<PortfolioPositionDto> Positions { get; init; }
        = [];

    public IReadOnlyList<string> RiskFactors { get; init; }
        = [];
}
