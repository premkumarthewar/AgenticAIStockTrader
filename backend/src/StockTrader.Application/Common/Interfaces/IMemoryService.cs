using StockTrader.Domain.Entities;

namespace StockTrader.Application.Common.Interfaces;

public interface IMemoryService
{
    Task SaveAsync(string category, string symbol, string content, string sourceAgent, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TradingMemory>> GetBySymbolAsync(
       string symbol,
       CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TradingMemory>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<string> GetMemoryContextAsync(string symbol, int maxRecords = 10, CancellationToken cancellationToken = default);
}
