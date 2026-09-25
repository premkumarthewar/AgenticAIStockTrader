namespace StockTrader.AI.Scoring;

/// <summary>
/// Turns an AI agent's decision and confidence (plus, when available, risk level and supporting/risk factor counts) into a single, deterministic 0-100 recommendation score and a human-readable, analyst-style rating. This is a pure calculation over data the agents already produce - it never calls the LLM, and it never changes the underlying BUY/HOLD/SELL decision, only how strongly that decision is expressed.
/// </summary>
internal static class RecommendationScoreCalculator
{
    public static (int Score, string Rating) Calculate(
        string decision,
        decimal confidence,
        string? riskLevel = null,
        int supportingFactorCount = 0,
        int riskFactorCount = 0)
    {
        decimal score = Math.Clamp(confidence, 0, 100);

        // Risk adjustment: a HIGH risk call is worth less than the raw confidence number alone suggests; a LOW risk call is worth a little more.
        score += riskLevel?.Trim().ToUpperInvariant() switch
        {
            "LOW" => 5m,
            "HIGH" => -15m,
            _ => 0m
        };

        // Factor-balance adjustment: more supporting factors than risk factors nudges the score up, and vice versa, capped so it can't dominate the confidence.
        int factorBalance = supportingFactorCount - riskFactorCount;

        score += Math.Clamp(factorBalance * 2m, -10m, 10m);

        score = Math.Clamp(score, 0, 100);

        int roundedScore = (int)Math.Round(score, MidpointRounding.AwayFromZero);

        string rating = BuildRating(decision, roundedScore);

        return (roundedScore, rating);
    }

    private static string BuildRating(string decision, int score)
    {
        string normalizedDecision = decision?.Trim().ToUpperInvariant() ?? "HOLD";

        if (normalizedDecision == "BUY")
        {
            return score switch
            {
                >= 80 => "Strong Buy",
                >= 60 => "Buy",
                >= 40 => "Weak Buy",
                _ => "Hold"
            };
        }

        if (normalizedDecision == "SELL")
        {
            return score switch
            {
                >= 80 => "Strong Sell",
                >= 60 => "Sell",
                >= 40 => "Weak Sell",
                _ => "Hold"
            };
        }

        return "Hold";
    }
}
