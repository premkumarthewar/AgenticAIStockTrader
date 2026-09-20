using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Portfolio;
using StockTrader.AI.Prompts;
using StockTrader.AI.Services;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Application.RiskManagement.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;
using System.Text;
using System.Text.Json;

namespace StockTrader.AI;

public sealed class TradingOrchestrator(IMarketAgent marketAgent, IResearchAgent researchAgent, ITradingDecisionAgent tradingDecisionAgent, IPortfolioAgent portfolioAgent,
    IWatchlistAgent watchlistAgent, IRiskManagementService riskManagementService, IPaperTradingService paperTradingService, Microsoft.SemanticKernel.Kernel kernel) : ITradingOrchestrator
{
    public async Task<Result<TradingDecisionDto>> AnalyzeAsync(
        AnalyzeStockRequest analyzeStockRequest,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(analyzeStockRequest.Symbol))
        {
            return Result<TradingDecisionDto>.Failure(
                new Error(
                    "BadRequest",
                    "Stock symbol is required."));
        }

        string normalizedSymbol =
            analyzeStockRequest.Symbol
                .Trim()
                .ToUpperInvariant();

        try
        {
            Task<Result<string>> marketTask =
                marketAgent.AnalyzeAsync(
                    analyzeStockRequest,
                    cancellationToken);

            Task<Result<string>> researchTask =
                researchAgent.ResearchAsync(
                    analyzeStockRequest,
                    cancellationToken);

            await Task.WhenAll(
                marketTask,
                researchTask);

            Result<string> marketResult =
                await marketTask;

            if (marketResult.IsFailure)
            {
                return Result<TradingDecisionDto>.Failure(
                    new Error(
                        "InternalServerError",
                        $"Market analysis failed: {marketResult.Error}"));
            }

            Result<string> researchResult =
                await researchTask;

            if (researchResult.IsFailure)
            {
                return Result<TradingDecisionDto>.Failure(
                    new Error(
                        "InternalServerError",
                        $"Company research failed: {researchResult.Error}"));
            }

            string? portfolioContext = null;
            string? watchlistContext = null;
            string? riskContext = null;

            Result<PaperPortfolioDto> paperPortfolioResult =
                await paperTradingService.GetPortfolioAsync(
                    cancellationToken);

            PaperPortfolioDto? portfolio =
                paperPortfolioResult.IsSuccess
                    ? paperPortfolioResult.Value
                    : null;

            if (portfolio is not null)
            {
                PortfolioAnalysisRequestDto portfolioRequest =
                    new()
                    {
                        Holdings =
                        [
                            .. portfolio.Positions.Select(p =>
                            new PortfolioHoldingDto
                            {
                                Symbol = p.Symbol,
                                Quantity = p.Quantity,
                                AverageCost = p.AveragePrice
                            })
                        ],

                        AvailableCash =
                            portfolio.CashBalance
                    };

                Result<PortfolioRecommendationDto> portfolioAnalysisResult =
                    await portfolioAgent.AnalyzeAsync(
                        portfolioRequest,
                        cancellationToken);

                if (portfolioAnalysisResult.IsSuccess)
                {
                    portfolioContext =
                        JsonSerializer.Serialize(
                            portfolioAnalysisResult.Value);
                }
            }

            WatchlistDto watchlistRequest =
                new()
                {
                    Symbols =
                    [
                        normalizedSymbol
                    ]
                };

            Result<WatchlistAnalysisDto> watchlistAnalysisResult =
                await watchlistAgent.AnalyzeAsync(
                    watchlistRequest,
                    cancellationToken);

            if (watchlistAnalysisResult.IsSuccess)
            {
                watchlistContext =
                    JsonSerializer.Serialize(
                        watchlistAnalysisResult.Value);
            }

            decimal existingExposure = 0m;

            if (portfolio is not null)
            {
                existingExposure =
                    portfolio.Positions
                        .Where(x =>
                            x.Symbol.Equals(
                                normalizedSymbol,
                                StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.MarketValue);
            }

            RiskRequestDto riskRequest =
                new()
                {
                    Symbol = normalizedSymbol,

                    CurrentPrice =
                        portfolio?
                            .Positions
                            .FirstOrDefault(x =>
                                x.Symbol.Equals(
                                    normalizedSymbol,
                                    StringComparison.OrdinalIgnoreCase))
                            ?.CurrentPrice ?? 0m,

                    CashBalance =
                        portfolio?.CashBalance ?? 0m,

                    PortfolioValue =
                        portfolio?.CurrentPortfolioValue ?? 0m,

                    ExistingExposure =
                        existingExposure
                };

            Result<RiskAssessmentDto> riskResult =
                await riskManagementService.AssessAsync(
                    riskRequest,
                    cancellationToken);

            if (riskResult.IsSuccess)
            {
                riskContext =
                    JsonSerializer.Serialize(
                        riskResult.Value);
            }

            string enrichedResearchAnalysis =
                BuildEnrichedResearchAnalysis(
                    researchResult.Value,
                    portfolioContext,
                    watchlistContext,
                    riskContext);

            Result<string> synthesisResult =
                await SynthesizeAsync(
                    normalizedSymbol,
                    marketResult.Value,
                    enrichedResearchAnalysis,
                    cancellationToken);

            if (synthesisResult.IsFailure)
            {
                return Result<TradingDecisionDto>.Failure(
                    new Error(
                        "InternalServerError",
                        $"Analysis synthesis failed: {synthesisResult.Error}"));
            }

            Result<TradingDecisionDto> decisionResult =
                await tradingDecisionAgent.DecideAsync(
                    normalizedSymbol,
                    synthesisResult.Value,
                    cancellationToken);

            if (decisionResult.IsFailure)
            {
                return Result<TradingDecisionDto>.Failure(
                    new Error(
                        "InternalServerError",
                        $"Trading decision generation failed: {decisionResult.Error}"));
            }

            return decisionResult;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<TradingDecisionDto>.Failure(
                new Error(
                    "InternalServerError",
                    $"Unable to complete trading analysis for {normalizedSymbol}: {ex.Message}"));
        }
    }
    private static string BuildCombinedAnalysis(string symbol, string marketAnalysis, string researchAnalysis)
    {
        return $"""
            COMBINED STOCK ANALYSIS
            =======================

            Symbol: {symbol}

            MARKET ANALYSIS
            ----------------
            {marketAnalysis}

            FUNDAMENTAL RESEARCH
            --------------------
            {researchAnalysis}

            END OF ANALYSIS
            """;
    }

    private async Task<Result<string>> SynthesizeAsync(string symbol, string marketAnalysis, string researchAnalysis, CancellationToken cancellationToken)
    {
        OrchestrationPrompt orchestrationPrompt = new(symbol, marketAnalysis, researchAnalysis);

        try
        {
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            chatHistory.AddSystemMessage(orchestrationPrompt.SystemPrompt);

            chatHistory.AddUserMessage(orchestrationPrompt.UserPrompt);

            OpenAIPromptExecutionSettings executionSettings = new();

            ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);

            ArgumentException.ThrowIfNullOrEmpty(response.Content);

            return Result<string>.Success(response.Content);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("InternalServerError", $"Unable to synthesize the agent analysis: {ex.Message}"));
        }
    }


    private static string BuildEnrichedResearchAnalysis(
        string researchAnalysis,
        string? portfolioContext,
        string? watchlistContext,
        string? riskContext)
    {
        StringBuilder builder = new();

        builder.AppendLine(researchAnalysis);

        builder.AppendLine();
        builder.AppendLine("PORTFOLIO CONTEXT");
        builder.AppendLine("-----------------");
        builder.AppendLine(
            string.IsNullOrWhiteSpace(portfolioContext)
                ? "No portfolio context available."
                : portfolioContext);

        builder.AppendLine();
        builder.AppendLine("WATCHLIST CONTEXT");
        builder.AppendLine("-----------------");
        builder.AppendLine(
            string.IsNullOrWhiteSpace(watchlistContext)
                ? "No watchlist context available."
                : watchlistContext);

        builder.AppendLine();
        builder.AppendLine("RISK CONTEXT");
        builder.AppendLine("------------");
        builder.AppendLine(
            string.IsNullOrWhiteSpace(riskContext)
                ? "No risk context available."
                : riskContext);

        return builder.ToString();
    }
}
