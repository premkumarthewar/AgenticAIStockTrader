using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Context;
using System.Text;

namespace StockTrader.AI.Memory
{
    public class MemorySummarizer(IChatCompletionService chatCompletionService, StockTraderDbContext dbContext) : IMemorySummarizer
    {
        public async Task RefreshSummaryAsync(string symbol, CancellationToken cancellationToken = default)
        {
            string normalizedSymbol = symbol.Trim().ToUpperInvariant();

            List<TradingMemory> memories = await dbContext.TradingMemories.Where(t => t.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)).OrderByDescending(t => t.CreatedOnUtc).Take(50).ToListAsync(cancellationToken);

            if (memories.Count == 0)
                return;

            StringBuilder memoryBuilder = new();

            foreach (TradingMemory memory in memories)
            {
                memoryBuilder.AppendLine($"[{memory.CreatedOnUtc:yyyy-MM-dd HH:mm:ss}]");

                memoryBuilder.AppendLine($"Category: {memory.Category}");

                memoryBuilder.AppendLine($"Agent: {memory.SourceAgent}");

                memoryBuilder.AppendLine(memory.Content);

                memoryBuilder.AppendLine();

                ChatHistory chatHistory = [];

                chatHistory.AddSystemMessage("""
                    You are a trading memory summarization assistant.

                    Summarize trading history.

                    Focus on:
                    - recurring recommendations
                    - previous conclusions
                    - major events
                    - notable outcomes

                    Keep the summary concise.
                    """);

                chatHistory.AddUserMessage(memoryBuilder.ToString());

                ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

                string summary = response.Content?.Trim() ?? "No summary available";

                MemorySummary? existingSummary = await dbContext.MemorySummaries.FirstOrDefaultAsync(m => m.Symbol.Equals(normalizedSymbol, StringComparison.OrdinalIgnoreCase), cancellationToken);

                if (existingSummary is null)
                    dbContext.MemorySummaries.Add(new MemorySummary
                    {
                        Id = Guid.NewGuid(),
                        Symbol = normalizedSymbol,
                        Summary = summary,
                        LastUpdatedUtc = DateTime.UtcNow,
                    });
                else
                {
                    existingSummary.Summary = summary;
                    existingSummary.LastUpdatedUtc = DateTime.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
