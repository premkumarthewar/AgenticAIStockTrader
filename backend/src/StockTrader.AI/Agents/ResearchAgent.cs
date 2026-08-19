using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Base;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Plugins.CompanyProfile;
using StockTrader.AI.Plugins.Financials;
using StockTrader.AI.Plugins.News;
using StockTrader.AI.Prompts;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents;

public class ResearchAgent : AgentBase, IResearchAgent
{
    public ResearchAgent(AgentContext context, CompanyProfilePlugin companyProfilePlugin, FinancialsPlugin financialsPlugin, NewsPlugin newsPlugin) : base(context)
    {
        ArgumentNullException.ThrowIfNull(companyProfilePlugin);
        ArgumentNullException.ThrowIfNull(financialsPlugin);
        ArgumentNullException.ThrowIfNull(newsPlugin);

        Kernel.Plugins.AddFromObject(companyProfilePlugin, "CompanyProfile");
        Kernel.Plugins.AddFromObject(financialsPlugin, "Financials");
        Kernel.Plugins.AddFromObject(newsPlugin, "News");
    }

    public async Task<Result<string>> ResearchAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(symbol))
            return Result<string>.Failure(new Error("BadRequest", "Stock symbol is required"));

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        try
        {
            Logger.LogInformation($"Starting company research for {normalizedSymbol}");

            IChatCompletionService chatCompletionService = Kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            chatHistory.AddSystemMessage(ResearchPrompt.SystemPrompt);

            DateTime newsFrom = DateTime.UtcNow.Date.AddDays(-30);

            ResearchPrompt researchPrompt = new(symbol, newsFrom, DateTime.UtcNow.Date);

            chatHistory.AddUserMessage(researchPrompt.UserPrompt);

            OpenAIPromptExecutionSettings executionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, Kernel, cancellationToken);

            string? research = response.Content;

            if (string.IsNullOrEmpty(research))
                return Result<string>.Failure(new Error("NotFound", $"AI model returned an empty research result for {normalizedSymbol}"));

            Logger.LogInformation($"Completed company research for {normalizedSymbol}");

            return Result<string>.Success(research);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while researching {normalizedSymbol}");

            return Result<string>.Failure(new Error("InternalServerError", $"Unable to research {normalizedSymbol}: {ex.Message}"));
        }

        throw new NotImplementedException();
    }
}
