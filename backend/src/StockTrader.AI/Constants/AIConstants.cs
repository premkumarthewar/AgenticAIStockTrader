namespace StockTrader.AI.Constants;

/// <summary>
/// Contains constants used in the AI module of the StockTrader application.
/// </summary>
public static class AIConstants
{
    public static class Models
    {
        public const string GPT41 = "gpt-4.1";
        public const string GPT41Mini = "gpt-4.1-mini";
    }

    public static class Agents
    {
        public const string Orchestrator = nameof(Orchestrator);
        public const string Market = nameof(Market);
        public const string Research = nameof(Research);
        public const string Portfolio = nameof(Portfolio);
        public const string Risk = nameof(Risk);
        public const string Execution = nameof(Execution);
    }

    public static class Plugins
    {
        public const string CompanyProfile = nameof(CompanyProfile);
        public const string StockQuote = nameof(StockQuote);
        public const string HistoricalPrice = nameof(HistoricalPrice);
        public const string FinancialMetrics = nameof(FinancialMetrics);
        public const string News = nameof(News);
        public const string Portfolio = nameof(Portfolio);
        public const string TradeExecution = nameof(TradeExecution);
    }
}
