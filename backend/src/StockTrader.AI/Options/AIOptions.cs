namespace StockTrader.AI.Options;

/// <summary>
/// Class representing the configuration options for AI services, including provider, model, API key, temperature, and maximum tokens.
/// </summary>
public sealed class AIOptions
{
    public const string SectionName = "AI";

    public string Provider { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public double Temperature { get; init; }

    public int MaxTokens { get; init; }
}
