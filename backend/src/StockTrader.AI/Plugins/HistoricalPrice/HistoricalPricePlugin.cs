using Microsoft.SemanticKernel;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.ComponentModel;
using System.Text.Json;

namespace StockTrader.AI.Plugins.HistoricalPrice;

/// <summary>
/// Provides historical stock price information to AI agents.
/// </summary>
public class HistoricalPricePlugin(IStockMarketService stockMarketService)
{
    /// <summary>
    /// Gets historical daily stock prices for a specified date range.
    /// </summary>
    /// <param name="symbol">The stock ticker symbol, for example AAPL or MSFT.</param>
    /// <param name="startDate">The start date of the historical price range.</param>
    /// <param name="endDate">The end date of the historical price range.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A JSON string representing the historical daily stock prices.</returns>
    /// <exception cref="ArgumentException"></exception>
    [KernelFunction("get_historical_prices")]
    [Description("Gets historical daily stock prices for a specified date range.")]
    public async Task<string> GetHistoricalPriceAsync([Description("The stock ticker symbol, for example AAPL or MSFT.")] string symbol,
        [Description("The start date of the historical price range.")] DateTime startDate,
        [Description("The end date of the historical price range.")] DateTime endDate, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol, nameof(symbol));

        if (startDate > endDate)
            throw new ArgumentException("Start date must be earlier than or equal to end date.");

        Result<IReadOnlyList<HistoricalPriceDto>> result = await stockMarketService.GetHistoricalPricesAsync(symbol, startDate, endDate, cancellationToken);

        if (result.IsFailure)
            return $"Unable to retrieve historical price for symbol '{symbol}': {result.Error}";

        return JsonSerializer.Serialize(result.Value);
    }
}
    