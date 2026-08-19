namespace StockTrader.Application.AI.Dtos;

public sealed record TradingDecisionDto
{
    public required string Symbol { get; init; }

    public required string Decision { get; init; }

    public required decimal Confidence { get; init; }

    public required string RiskLevel { get; init; }

    public decimal? TargetBuyPrice { get; set; }

    public decimal? TargetSellPrice { get; set; }

    public required string Reasoning { get; init; }

    public IReadOnlyList<string> SupportingFactors { get; init; }
        = [];

    public IReadOnlyList<string> RiskFactors { get; init; }
        = [];
}