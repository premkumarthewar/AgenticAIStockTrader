using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Base;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Prompts;
using StockTrader.Application.AI.Dtos;
using StockTrader.Shared.Results;
using System.Text.Json;

namespace StockTrader.AI.Agents;

public sealed class TradingDecisionAgent(AgentContext context) : AgentBase(context), ITradingDecisionAgent
{
    public async Task<Result<TradingDecisionDto>> DecideAsync(string symbol, string integratedAnalysis, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        ArgumentException.ThrowIfNullOrEmpty(integratedAnalysis);

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        TradingDecisionPrompt tradingDecisionPrompt = new(normalizedSymbol, integratedAnalysis);

        try
        {
            IChatCompletionService chatCompletionService = Kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            chatHistory.AddSystemMessage(tradingDecisionPrompt.SystemPrompt);

            chatHistory.AddUserMessage(tradingDecisionPrompt.UserPrompt);

            OpenAIPromptExecutionSettings executionSettings = new();

            ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, Kernel, cancellationToken);

            if (string.IsNullOrEmpty(response.Content))
                return Result<TradingDecisionDto>.Failure(new Error("NotFound", "Trading Decision model returned and empty response"));

            TradingDecisionDto? decision = JsonSerializer.Deserialize<TradingDecisionDto>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (decision is null)
                return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", "Unable to parse the trading decision returned by the AI model"));

            if (!IsValidDecision(decision.Decision))
                return Result<TradingDecisionDto>.Failure(new Error("InvalidResponse", $"Invalid trading decision: {decision.Decision}"));

            if (decision.Confidence < 0 || decision.Confidence > 100)
                return Result<TradingDecisionDto>.Failure(new Error("InvalidResponse", "Trading decision confidence must be between 0 & 100"));

            if (decision.TargetBuyPrice.HasValue && decision.TargetBuyPrice <= 0)
                return Result<TradingDecisionDto>.Failure(new Error("InvalidResponse", "Target buy price must be greater than zero."));

            if (decision.TargetSellPrice.HasValue && decision.TargetSellPrice <= 0)
                return Result<TradingDecisionDto>.Failure(new Error("InvalidResponse", "Target sell price must be greater than zero."));

            if (decision.TargetBuyPrice.HasValue && decision.TargetSellPrice.HasValue && decision.TargetSellPrice <= decision.TargetBuyPrice)
                return Result<TradingDecisionDto>.Failure(new Error("InvalidResponse", "Target sell price must be greater than target buy price."));

            return Result<TradingDecisionDto>.Success(decision with
            {
                Symbol = normalizedSymbol
            });
        }
        catch (JsonException ex)
        {
            return Result<TradingDecisionDto>.Failure(new Error("ParserError", $"Unable to parse trading decision JSON: {ex.Message}"));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<TradingDecisionDto>.Failure(new Error("InternalServerError", $"Unable to generate trading decision: {ex.Message}"));
        }
    }

    private static bool IsValidDecision(
        string? decision)
    {
        return decision?.Trim().ToUpperInvariant() switch
        {
            "BUY" => true,
            "HOLD" => true,
            "SELL" => true,
            _ => false
        };
    }
}
