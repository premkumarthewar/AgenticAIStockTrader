using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.SmartApi.Models;

/// <summary>
/// placeOrder's own response - just script/orderid, no status. Use SmartApiOrderBookEntry
/// (from getOrderBook) for the actual current state.
/// </summary>
public sealed record SmartApiOrderData
{
    [JsonPropertyName("script")]
    public string? Script { get; init; }

    [JsonPropertyName("orderid")]
    public string? OrderId { get; init; }

    [JsonPropertyName("uniqueorderid")]
    public string? UniqueOrderId { get; init; }
}

public sealed record SmartApiOrderBookEntry
{
    [JsonPropertyName("orderid")]
    public string? OrderId { get; init; }

    [JsonPropertyName("tradingsymbol")]
    public string? TradingSymbol { get; init; }

    [JsonPropertyName("transactiontype")]
    public string? TransactionType { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; init; }

    [JsonPropertyName("filledshares")]
    public string? FilledShares { get; init; }

    [JsonPropertyName("averageprice")]
    public string? AveragePrice { get; init; }

    [JsonPropertyName("updatetime")]
    public string? UpdateTime { get; init; }
}
