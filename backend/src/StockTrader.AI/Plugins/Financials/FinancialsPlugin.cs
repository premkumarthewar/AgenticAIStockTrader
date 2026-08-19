using Microsoft.SemanticKernel;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.ComponentModel;
using System.Text.Json;

namespace StockTrader.AI.Plugins.Financials;

public class FinancialsPlugin(IStockMarketService stockMarketService)
{
    [KernelFunction("get_financials")]
    [Description("Gets the latest available financial metrics for a stock symbol, including revenue, net income, EPS, margins and returns.")]
    public async Task<string> GetFinancialsAsync([Description("Stock ticker symbol such as AAPL or MSFT")] string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        Result<FinancialsDto> result = await stockMarketService.GetFinancialsAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return $"Unable to retrieve financial information for {symbol}: {result.Error}";

        return JsonSerializer.Serialize(result.Value);
    }
}
