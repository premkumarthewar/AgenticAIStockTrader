using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Alpaca.Models;

public sealed record AlpacaAccountResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("cash")]
    public string? Cash { get; init; }

    [JsonPropertyName("portfolio_value")]
    public string? PortfolioValue { get; init; }

    [JsonPropertyName("buying_power")]
    public string? BuyingPower { get; init; }

    [JsonPropertyName("trading_blocked")]
    public bool TradingBlocked { get; init; }

    [JsonPropertyName("account_blocked")]
    public bool AccountBlocked { get; init; }
}
