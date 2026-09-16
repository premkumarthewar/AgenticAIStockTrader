namespace StockTrader.Application.RiskManagement.Dtos;

public sealed record RiskAssessmentDto
{
    public bool IsApproved { get; init; }

    public string Reason { get; init; } = string.Empty;

    public decimal RecommendedQuantity { get; init; }

    public decimal RecommendedCapitalAllocation { get; init; }

    public decimal PortfolioExposurePercentage { get; init; }

    public decimal RiskScore { get; init; }
}