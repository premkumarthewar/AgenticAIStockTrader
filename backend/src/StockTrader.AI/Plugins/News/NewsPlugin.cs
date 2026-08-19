using Microsoft.SemanticKernel;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.ComponentModel;
using System.Text.Json;

namespace StockTrader.AI.Plugins.News;

public class NewsPlugin(IStockMarketService stockMarketService)
{
    [KernelFunction("get_company_news")]
    [Description("Gets recent news articles related to a stock symbol.")]
    public async Task<string> GetCompanyNewsAsync(
        [Description("Stock ticker symbol such as AAPL or  MSFT.")] string symbol,
        [Description("Start date in yyyy-MM-dd format.")] DateTime from, [Description("End date in yyyy-MM-dd format.")] DateTime to, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        if (from > to)
            return "Start date cannot be greater than end date.";

        Result<IReadOnlyList<NewsArticleDto>> result = await stockMarketService.GetNewsAsync(symbol, from, to, cancellationToken);

        if (result.IsFailure)
            return $"Unable to retrieve news for {symbol}: {result.Error}";

        return JsonSerializer.Serialize(result.Value);
    }
}
