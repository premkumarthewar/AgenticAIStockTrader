using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Application.RiskManagement.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;
using System.Text.Json;

namespace StockTrader.AI.Services;

public class TradingAdvisorService(IAgentFactory agentFactory, ITradingOrchestrator tradingOrchestrator, IMemoryService memoryService, IMemorySummarizer memorySummarizer, IRiskManagementService riskManagementService, IPaperTradingService paperTradingService) : ITradingAdvisorService
{
    public async Task<Result<TradingDecisionDto>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return Result<TradingDecisionDto>.Failure(new Error("BadRequest", "Stock symbol is required."));

        Result<TradingDecisionDto> tradingDecision = await tradingOrchestrator.AnalyzeAsync(request, cancellationToken);

        if (tradingDecision.IsFailure)
            return tradingDecision;

        TradingDecisionDto decision = tradingDecision.Value;

        PaperPortfolioDto? portfolio = null;

        Result<PaperPortfolioDto> portfolioResult = await paperTradingService.GetPortfolioAsync(cancellationToken);

        if (portfolioResult.IsSuccess)
            portfolio = portfolioResult.Value;

        decimal currentPrice = decision.TargetBuyPrice ?? decision.TargetSellPrice ?? 0m;

        decimal existingExposure = 0m;

        if (portfolio is not null)
            existingExposure = portfolio.Positions.Where(x => x.Symbol.Equals(request.Symbol, StringComparison.OrdinalIgnoreCase)).Sum(x => x.MarketValue);

        RiskRequestDto riskRequest = new()
        {
            Symbol = request.Symbol,
            CurrentPrice = currentPrice,
            CashBalance = portfolio?.CashBalance ?? 100000m,
            PortfolioValue = portfolio?.CurrentPortfolioValue ?? 100000m,
            ExistingExposure = existingExposure
        };

        Result<RiskAssessmentDto> riskResult = await riskManagementService.AssessAsync(riskRequest, cancellationToken);

        if (riskResult.IsSuccess && !riskResult.Value.IsApproved && decision.Decision.Equals("BUY", StringComparison.OrdinalIgnoreCase))
        {
            decision = decision with
            {
                Decision = "HOLD",
                RiskLevel = "HIGH",
                Reasoning =
                    $"{decision.Reasoning} " +
                    $"Risk Override: {riskResult.Value.Reason}"
            };
        }

        await memoryService.SaveAsync("TradingDecision", request.Symbol, JsonSerializer.Serialize(tradingDecision), "TradingDecisionAgent", cancellationToken);

        int memoryCount = (await memoryService.GetBySymbolAsync(request.Symbol, cancellationToken)).Count;

        if (memoryCount % 25 == 0)
            await memorySummarizer.RefreshSummaryAsync(request.Symbol, cancellationToken);

        return Result<TradingDecisionDto>.Success(decision);
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
