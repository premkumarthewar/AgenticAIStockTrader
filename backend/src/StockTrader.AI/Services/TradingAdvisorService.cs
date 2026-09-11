using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Memory;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;
using System.Text.Json;

namespace StockTrader.AI.Services;

public class TradingAdvisorService(IAgentFactory agentFactory, ITradingOrchestrator tradingOrchestrator, IMemoryService memoryService, IMemorySummarizer memorySummarizer) : ITradingAdvisorService
{
    public async Task<Result<TradingDecisionDto>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return Result<TradingDecisionDto>.Failure(new Error("BadRequest", "Stock symbol is required."));

        Result<TradingDecisionDto> tradingDecision = await tradingOrchestrator.AnalyzeAsync(request, cancellationToken);

        await memoryService.SaveAsync("TradingDecision", request.Symbol, JsonSerializer.Serialize(tradingDecision.Value), "TradingDecisionAgent", cancellationToken);

        int memoryCount = (await memoryService.GetBySymbolAsync(request.Symbol, cancellationToken)).Count;

        if (memoryCount % 25 == 0)
            await memorySummarizer.RefreshSummaryAsync(request.Symbol, cancellationToken);

        return tradingDecision;
    }

    public async Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        IMarketAgent marketAgent = agentFactory.CreateMarketAgent();

        Result<string> analysis = await marketAgent.AnalyzeAsync(request, cancellationToken);

        await memoryService.SaveAsync("MarketAnalysis", request.Symbol, JsonSerializer.Serialize(analysis.Value), "MarketAgent", cancellationToken);

        int memoryCount = (await memoryService.GetBySymbolAsync(request.Symbol, cancellationToken)).Count;

        if (memoryCount % 25 == 0)
            await memorySummarizer.RefreshSummaryAsync(request.Symbol, cancellationToken);

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

        await memoryService.SaveAsync("Research", request.Symbol, JsonSerializer.Serialize(research.Value), "ResearchAgent", cancellationToken);

        int memoryCount = (await memoryService.GetBySymbolAsync(request.Symbol, cancellationToken)).Count;

        if (memoryCount % 25 == 0)
            await memorySummarizer.RefreshSummaryAsync(request.Symbol, cancellationToken);

        return Result<AnalyzeStockResponse>.Success(new AnalyzeStockResponse
        {
            Analysis = research.Value
        });
    }

    public async Task<Result<PortfolioRecommendationDto>> AnalyzePortfolioAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        IPortfolioAgent portfolioAgent = agentFactory.CreatePortfolioAgent();

        return await portfolioAgent.AnalyzeAsync(request, cancellationToken);
    }

    public async Task<Result<WatchlistAnalysisDto>> AnalyzeWatchlistAsync(WatchlistDto request, CancellationToken cancellationToken = default)
    {
        IWatchlistAgent watchlistAgent = agentFactory.CreateWatchlistAgent();

        return await watchlistAgent.AnalyzeAsync(request, cancellationToken);
    }

    public async Task<Result<MemoryResponseDto>> GetMemoryAsync(string symbol, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TradingMemory> records = await memoryService.GetBySymbolAsync(symbol, cancellationToken);

        MemoryResponseDto dto = new()
        {
            Entries = [.. records.Select(x => x.Content)]
        };

        return Result<MemoryResponseDto>.Success(dto);
    }
}
