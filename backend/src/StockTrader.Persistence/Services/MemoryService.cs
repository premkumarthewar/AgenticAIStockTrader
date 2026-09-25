using Microsoft.EntityFrameworkCore;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Context;
using System.Text;

namespace StockTrader.Persistence.Services;

public sealed class MemoryService(StockTraderDbContext dbContext) : IMemoryService
{
    public async Task<IReadOnlyCollection<TradingMemory>> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        return await dbContext.TradingMemories.Where(x => x.Symbol == normalizedSymbol).OrderByDescending(x => x.CreatedOnUtc).ToListAsync(cancellationToken);
    }

    public async Task<string> GetMemoryContextAsync(string symbol, int maxRecords = 10, CancellationToken cancellationToken = default)
    {
        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        MemorySummary? summary = await dbContext.MemorySummaries.FirstOrDefaultAsync(x => x.Symbol == normalizedSymbol, cancellationToken);

        List<TradingMemory> recentMemories = await dbContext.TradingMemories.Where(x => x.Symbol == normalizedSymbol).OrderByDescending(x => x.CreatedOnUtc).Take(maxRecords).ToListAsync(cancellationToken);

        StringBuilder builder = new();

        builder.AppendLine("=== MEMORY SUMMARY ===");
        builder.AppendLine();

        builder.AppendLine(summary?.Summary ?? "No summary available.");

        builder.AppendLine();
        builder.AppendLine("=== RECENT MEMORY ===");
        builder.AppendLine();

        foreach (TradingMemory memory in recentMemories)
        {
            builder.AppendLine($"[{memory.CreatedOnUtc:yyyy-MM-dd HH:mm:ss}]");

            builder.AppendLine($"Category: {memory.Category}");

            builder.AppendLine($"Agent: {memory.SourceAgent}");

            builder.AppendLine(memory.Content);

            builder.AppendLine();
        }

        return builder.ToString();
    }

    public async Task<IReadOnlyCollection<TradingMemory>> GetRecentAsync(int count, CancellationToken cancellationToken = default) => await dbContext.TradingMemories.OrderByDescending(x => x.CreatedOnUtc).Take(count).ToListAsync(cancellationToken);

    public async Task SaveAsync(string category, string symbol, string content, string sourceAgent, CancellationToken cancellationToken = default)
    {
        dbContext.TradingMemories.Add(new TradingMemory
        {
            Id = Guid.NewGuid(),
            Category = category,
            Symbol = symbol.Trim().ToUpperInvariant(),
            Content = content,
            SourceAgent = sourceAgent,
            CreatedOnUtc = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
