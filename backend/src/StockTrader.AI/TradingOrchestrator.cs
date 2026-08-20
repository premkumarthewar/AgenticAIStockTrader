using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Prompts;
using StockTrader.Application.AI.Dtos;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI;

public sealed class TradingOrchestrator(IMarketAgent marketAgent, IResearchAgent researchAgent, ITradingDecisionAgent tradingDecisionAgent, Microsoft.SemanticKernel.Kernel kernel) : ITradingOrchestrator
{
    public async Task<Result<TradingDecisionDto>> AnalyzeAsync(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(analyzeStockRequest.Symbol))
        {
            return Result<TradingDecisionDto>.Failure(new Error("BadRequest", "Stock symbol is required."));
        }
        string normalizedSymbol = analyzeStockRequest.Symbol.Trim().ToUpperInvariant();

        try
        {
            Task<Result<string>> marketTask = marketAgent.AnalyzeAsync(analyzeStockRequest, cancellationToken);

            Task<Result<string>> researchTask = researchAgent.ResearchAsync(analyzeStockRequest, cancellationToken);

            await Task.WhenAll(marketTask, researchTask);

            Result<string> marketResult = await marketTask;

            if (marketResult.IsFailure)
                return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Market analysis failed: {marketResult.Error}"));

            Result<string> researchResult = await researchTask;

            if (researchResult.IsFailure)
                return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Company research failed: {researchResult.Error}"));

            Result<string> synthesisResult = await SynthesizeAsync(normalizedSymbol, marketResult.Value, researchResult.Value, cancellationToken);

            if (synthesisResult.IsFailure)
                return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Anlysis synthesis failed: {synthesisResult.Error}"));

            Result<TradingDecisionDto> decisionResult = await tradingDecisionAgent.DecideAsync(normalizedSymbol, synthesisResult.Value, cancellationToken);

            if (decisionResult.IsFailure)
                return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Trading decision generation failed: {decisionResult.Error}"));

            return decisionResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Unable to complete trading analysis for {normalizedSymbol}: {ex.Message}"));
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
}
