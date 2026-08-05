using System.ComponentModel.DataAnnotations;

namespace StockTrader.AI.Options;

/// <summary>
/// Class representing the configuration options for AI services, including provider, model, API key, temperature, and maximum tokens.
/// </summary>
public sealed class AIOptions
{
    public const string SectionName = "AI";

    public string Provider { get; init; } = string.Empty;

    [Required]
    public string Model { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Range(0, 2)]
    public double Temperature { get; init; } = 0.2;

    public int MaxTokens { get; init; } = 4096;

    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromMinutes(2);
}
