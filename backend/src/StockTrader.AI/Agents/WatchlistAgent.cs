using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Prompts;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System;
using System.Collections.Generic;
using System.Text;
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
                Task<Result<StockQuoteDto>> quoteResult = stockMarketService.GetQuoteAsync(symbol, cancellationToken);

                if (quoteResult.IsFaulted)
                    continue;

                Result<StockQuoteDto> quote = quoteResult.Result;

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

            WatchlistPrompt watchlistPrompt = new(alertsJson);

            chatHistory.AddSystemMessage(watchlistPrompt.SystemPrompt);

            chatHistory.AddUserMessage(watchlistPrompt.UserPrompt);

            ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            List<string> recommendations = [response.Content ?? "No recommendations available"];

            WatchlistAnalysisDto result = new()
            {
                Alerts = alerts,
                Recommendations = recommendations,
            };

            return Result<WatchlistAnalysisDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<WatchlistAnalysisDto>.Failure(new Error("InternalServerError", ex.Message));
        }
    }
}
