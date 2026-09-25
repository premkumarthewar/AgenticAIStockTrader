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

    public decimal RecommendedQuantity { get; init; }

    public IReadOnlyList<string> SupportingFactors { get; init; }
        = [];

    public IReadOnlyList<string> RiskFactors { get; init; }
        = [];

    /// <summary>
    /// Deterministic 0-100 composite score derived from Confidence, RiskLevel, and the balance of SupportingFactors vs RiskFactors. Computed after the AI's decision is parsed and validated - the LLM never produces this value itself, so it is not "required" for JSON deserialization of the raw model response.
    /// </summary>
    public int RecommendationScore { get; init; }

    /// <summary>
    /// Analyst-style rating derived from Decision and RecommendationScore: one of "Strong Buy", "Buy", "Weak Buy", "Hold", "Weak Sell", "Sell", "Strong Sell".
    /// </summary>
    public string Rating { get; init; }
        = "Hold";
}