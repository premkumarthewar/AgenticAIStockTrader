using Microsoft.SemanticKernel;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.ComponentModel;
using System.Text.Json;

namespace StockTrader.AI.Plugins.Quotes;

/// <summary>
/// Provides current stock quote information to AI agents.
/// </summary>
public sealed class StockQuotePlugin(IStockMarketService stockMarketService)
{

    [KernelFunction("get_stock_quote")]
    [Description("Gets the latest stock price, change, percentage change, and other relevant information for a given stock symbol.")]
    public async Task<string> GetStockQuoteAsync([Description("The stock ticker symbol, for example AAPL or MSFT.")] string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol, nameof(symbol));

        Result<StockQuoteDto> result = await stockMarketService.GetQuoteAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return $"Unable to retrieve stock quote for symbol '{symbol}': {result.Error}";

        return JsonSerializer.Serialize(result.Value);
    }
}
