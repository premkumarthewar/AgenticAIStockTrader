using StockTrader.AI.Agents;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Responses;

namespace StockTrader.AI.Services;

public class TradingAdvisorService(MarketAgent marketAgent) : ITradingAdvisorService
{
    public async Task<AnalyzeStockResponse> AnalyzeStockAsync(string symbol, CancellationToken cancellationToken)
    {
        string analysis = await marketAgent.AnalyzeAsync(symbol, cancellationToken);

        return new AnalyzeStockResponse
        {
            Analysis = analysis
        };
    }
}
