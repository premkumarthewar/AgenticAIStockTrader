using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents.Base;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Plugins.CompanyProfile;
using StockTrader.AI.Plugins.HistoricalPrice;
using StockTrader.AI.Plugins.Quotes;
using StockTrader.AI.Prompts;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents;

public sealed class MarketAgent : AgentBase, IMarketAgent
{
    public MarketAgent(
        AgentContext context,
        CompanyProfilePlugin companyProfilePlugin,
        StockQuotePlugin stockQuotePlugin,
        HistoricalPricePlugin historicalPricePlugin)
        : base(context)
    {
        ArgumentNullException.ThrowIfNull(companyProfilePlugin);
        ArgumentNullException.ThrowIfNull(stockQuotePlugin);
        ArgumentNullException.ThrowIfNull(historicalPricePlugin);

        Kernel.Plugins.AddFromObject(
            companyProfilePlugin,
            "CompanyProfile");

        Kernel.Plugins.AddFromObject(
            stockQuotePlugin,
            "Quotes");

        Kernel.Plugins.AddFromObject(
            historicalPricePlugin,
            "HistoricalPrice");
    }

    public async Task<Result<string>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Symbol))
            return Result<string>.Failure(new Error("BadRequest", "Stock symbol is required"));

        string normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();

        try
        {
            OpenAIPromptExecutionSettings executionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            MarketPrompt marketPrompt = new(request.Symbol, request.TimeInterval);

            string prompt = marketPrompt.SystemPrompt;

            KernelArguments arguments = new(executionSettings)
            {
                ["symbol"] = normalizedSymbol
            };

            FunctionResult result = await Kernel.InvokePromptAsync(prompt, arguments, null, null, null, cancellationToken);

            string? analysis = result.GetValue<string>();

            if (string.IsNullOrEmpty(analysis))
                return Result<string>.Failure(new Error("NotFound", $"AI model returned an empty analysis for {normalizedSymbol}"));

            return Result<string>.Success(analysis);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while analyzing stock symbol {normalizedSymbol}");

            return Result<string>.Failure(new Error("InternalServerError", $"Unable to analyze {normalizedSymbol}: {ex.Message}"));
        }
    }
}
