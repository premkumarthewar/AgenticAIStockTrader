namespace StockTrader.AI.Prompts;

public class WatchlistPrompt(string alertsJson)
{
    public string SystemPrompt = """
        You are a stock market monitoring assistant.

        Review watchlist stocks.

        Identify:
        - unusual price movement
        - significant gains
        - significant losses
        - elevated risk

        Generate concise actionable insights.

        Do not provide financial advice.
        """;

    public string UserPrompt = $"""
                Analyze these alerts:

                {alertsJson}

                Return 3 concise recommendations.
                """;
}
