using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Prompts;
using StockTrader.AI.Scoring;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.Text.Json;

namespace StockTrader.AI.Agents;

public class WatchlistAgent(Microsoft.SemanticKernel.Kernel kernel, IStockMarketService stockMarketService) : IWatchlistAgent
{
    public async Task<Result<WatchlistAnalysisDto>> AnalyzeAsync(WatchlistDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Symbols.Count == 0)
                return Result<WatchlistAnalysisDto>.Failure(new Error("BadRequest", "No watchlist symbols supplied"));

            List<AlertDto> alerts = [];

            foreach (string symbol in request.Symbols)
            {
                // Fetching sequentially and per-symbol so that one bad/unknown symbol in the watchlist is skipped instead of failing the whole request.
                Result<StockQuoteDto> quote = await stockMarketService.GetQuoteAsync(symbol, cancellationToken);

                if (quote.IsFailure)
                    continue;

                decimal changePercent = quote.Value.PercentChange;

                if (changePercent >= 5)
                {
                    alerts.Add(new AlertDto
                    {
                        Symbol = symbol,
                        AlertType = "PRICE_SURGE",
                        Message = $"Price increased by {changePercent:F2}%",
                        TriggeredAt = DateTime.UtcNow,
                    });
                }
                else if (changePercent <= -5)
                {
                    alerts.Add(new AlertDto
                    {
                        Symbol = symbol,
                        AlertType = "PRICE_DROP",
                        Message = $"Price decreased by {changePercent:F2}%",
                        TriggeredAt = DateTime.UtcNow
                    });
                }
            }

            string alertsJson = JsonSerializer.Serialize(alerts);

            IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            WatchlistPrompt watchlistPrompt = new(request.Symbols, alertsJson);

            chatHistory.AddSystemMessage(watchlistPrompt.SystemPrompt);

            chatHistory.AddUserMessage(watchlistPrompt.UserPrompt);

            ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(response.Content))
                return Result<WatchlistAnalysisDto>.Failure(new Error("InternalServerError", "Watchlist agent returned an empty response"));

            List<WatchlistAssessment>? assessments;

            try
            {
                assessments = JsonSerializer.Deserialize<List<WatchlistAssessment>>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                return Result<WatchlistAnalysisDto>.Failure(new Error("ParserError", $"Unable to parse watchlist recommendations JSON: {ex.Message}"));
            }

            if (assessments is null || assessments.Count == 0)
                return Result<WatchlistAnalysisDto>.Failure(new Error("InternalServerError", "Unable to parse the watchlist recommendations returned by the AI model"));

            List<WatchlistRecommendationDto> recommendations = [];

            foreach (WatchlistAssessment assessment in assessments)
            {
                if (string.IsNullOrWhiteSpace(assessment.Symbol))
                    continue;

                decimal confidence = Math.Clamp(assessment.Confidence, 0, 100);

                (int score, string rating) = RecommendationScoreCalculator.Calculate(
                    assessment.Action,
                    confidence);

                recommendations.Add(new WatchlistRecommendationDto
                {
                    Symbol = assessment.Symbol.Trim().ToUpperInvariant(),
                    Action = assessment.Action,
                    Score = score,
                    Rating = rating,
                    Summary = assessment.Summary
                });
            }

            WatchlistAnalysisDto result = new()
            {
                Alerts = alerts,
                Recommendations = recommendations,
            };

            return Result<WatchlistAnalysisDto>.Success(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<WatchlistAnalysisDto>.Failure(new Error("InternalServerError", ex.Message));
        }
    }

    /// <summary>
    /// Raw shape returned by the LLM for one symbol, before the deterministic score and
    /// rating are computed and mapped onto the public WatchlistRecommendationDto.
    /// </summary>
    private sealed class WatchlistAssessment
    {
        public string Symbol { get; init; } = string.Empty;

        public string Action { get; init; } = "HOLD";

        public decimal Confidence { get; init; }

        public string Summary { get; init; } = string.Empty;
    }
}
