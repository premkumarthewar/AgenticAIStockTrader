using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Prompts;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.Text.Json;

namespace StockTrader.AI.Agents;

public class PortfolioAgent(Microsoft.SemanticKernel.Kernel kernel, IStockMarketService stockMarketService) : IPortfolioAgent
{
    public async Task<Result<PortfolioRecommendationDto>> AnalyzeAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Result<PortfolioRecommendationDto>.Failure(new Error("BadRequest", "Portfolio request is required"));

        if (request.Holdings.Count == 0)
            return Result<PortfolioRecommendationDto>.Failure(new Error("BadRequest", "At least one holding is required"));

        try
        {
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            string holdingsJson = JsonSerializer.Serialize(request.Holdings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            ChatHistory chatHistory = [];

            PortfolioPrompt portfolioPrompt = new(request.AvailableCash, holdingsJson);

            chatHistory.AddSystemMessage(portfolioPrompt.SystemPrompt);

            chatHistory.AddUserMessage(portfolioPrompt.UserPrompt);

            OpenAIPromptExecutionSettings executionSettings = new()
            {
                Temperature = 0.2
            };

            ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);

            if (string.IsNullOrEmpty(response.Content))
                return Result<PortfolioRecommendationDto>.Failure(new Error("InternalServerError", "Portfolio Agent returned an empty response"));

            PortfolioRecommendationDto? recommendation = JsonSerializer.Deserialize<PortfolioRecommendationDto>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            List<PortfolioPositionDto> positions = await BuildPositionsAsync(request, cancellationToken);

            decimal portfolioValue = 0.0m, totalAssets = 0.0m, cashPercentage = 0.0m;

            if (positions.Count > 0)
            {
                portfolioValue = positions.Sum(p => p.MarketValue);
                totalAssets = portfolioValue + request.AvailableCash;

                positions = [.. positions.Select(position => position with
                {
                    WeightPercentage = totalAssets == 0 ? 0 : Math.Round(position.MarketValue / totalAssets * 100m, 2)
                })];

                cashPercentage = totalAssets == 0 ? 0 : Math.Round(request.AvailableCash / totalAssets * 100m, 2);
            }

            string portfolioJson = JsonSerializer.Serialize(positions, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            if (recommendation is null)
                return Result<PortfolioRecommendationDto>.Failure(new Error("InternalServerError", "Unable to parse portfolio recommendation"));

            recommendation = recommendation with
            {
                PortfolioValue = portfolioValue,
                CashPercentage = cashPercentage,
                Positions = positions,
            };

            return Result<PortfolioRecommendationDto>.Success(recommendation);
        }
        catch (JsonException ex)
        {
            return Result<PortfolioRecommendationDto>.Failure(new Error("InternalServerError", $"Portfolio JSON parsing failed: {ex.Message}"));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<PortfolioRecommendationDto>.Failure(new Error("InternalServerError", $"Portfolio analysis failed: {ex.Message}"));
        }

        throw new NotImplementedException();
    }

    private async Task<List<PortfolioPositionDto>> BuildPositionsAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        List<PortfolioPositionDto> positions = [];

        foreach (PortfolioHoldingDto holding in request.Holdings)
        {
            Result<StockQuoteDto> quote = await stockMarketService.GetQuoteAsync(holding.Symbol, cancellationToken);

            decimal marketValue = quote.Value.CurrentPrice * holding.Quantity;

            decimal costBasis = holding.AverageCost * holding.Quantity;

            decimal pnl = marketValue - costBasis;

            positions.Add(new PortfolioPositionDto
            {
                Symbol = holding.Symbol,
                Quantity = holding.Quantity,
                AverageCost = holding.AverageCost,
                CurrentPrice = quote.Value.CurrentPrice,
                MarketValue = marketValue,
                CostBasis = costBasis,
                UnrealizedPnL = pnl
            });
        }

        return positions;
    }
}
