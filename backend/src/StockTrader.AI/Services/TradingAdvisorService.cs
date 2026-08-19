using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

public class TradingAdvisorService(IAgentFactory agentFactory, ITradingOrchestrator tradingOrchestrator) : ITradingAdvisorService
{
    public async Task<Result<TradingDecisionDto>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return Result<TradingDecisionDto>.Failure(new Error("BadRequest", "Stock symbol is required."));

        return await tradingOrchestrator.AnalyzeAsync(request, cancellationToken);
    }

    public async Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        IMarketAgent marketAgent = agentFactory.CreateMarketAgent();

        Result<string> analysis = await marketAgent.AnalyzeAsync(request, cancellationToken);

        return Result<AnalyzeStockResponse>.Success(new AnalyzeStockResponse
        {
            Analysis = analysis.Value
        });
    }

    public async Task<Result<AnalyzeStockResponse>> ResearchAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Symbol))
            return Result<AnalyzeStockResponse>.Failure(new Error("BadRequest", "Stock symbol is required"));

        IResearchAgent researchAgent = agentFactory.CreateResearchAgent();

        Result<string> research = await researchAgent.ResearchAsync(request, cancellationToken);

        return Result<AnalyzeStockResponse>.Success(new AnalyzeStockResponse
        {
            Analysis = research.Value
        });
    }
}
