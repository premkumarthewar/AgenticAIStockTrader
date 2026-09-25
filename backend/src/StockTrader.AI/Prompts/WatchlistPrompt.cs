namespace StockTrader.AI.Prompts;

public class WatchlistPrompt(IReadOnlyList<string> symbols, string alertsJson)
{
    public string SystemPrompt = """
        You are a stock market monitoring assistant.

        Review watchlist stocks and their recent alerts.

        For every symbol supplied, assess:
        - unusual price movement
        - significant gains or losses
        - elevated risk

        Do not provide personalized financial advice.
        Do not fabricate information not present in the supplied alerts.

        Return ONLY a valid JSON array, with exactly one entry per supplied symbol,
        matching this structure:

        [
          {
            "symbol": "string",
            "action": "BUY | HOLD | SELL",
            "confidence": 0,
            "summary": "string"
          }
        ]
        """;

    public string UserPrompt = $"""
                Watchlist symbols: {string.Join(", ", symbols)}

                Recent alerts:
                {alertsJson}

                Provide exactly one assessment per symbol listed above.
                """;
}
