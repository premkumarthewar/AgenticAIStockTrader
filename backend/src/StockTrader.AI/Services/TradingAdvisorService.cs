using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

public class TradingAdvisorService(IAgentFactory agentFactory) : ITradingAdvisorService
{
    public async Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        IMarketAgent marketAgent = agentFactory.CreateMarketAgent();

        Result<string> analysis = await marketAgent.AnalyzeAsync(request, cancellationToken);

        return Result<AnalyzeStockResponse>.Success(new AnalyzeStockResponse
        {
            Analysis = analysis.Value
        });
    }
}
