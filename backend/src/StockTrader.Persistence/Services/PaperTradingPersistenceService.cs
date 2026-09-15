using Microsoft.EntityFrameworkCore;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Context;

namespace StockTrader.Persistence.Services;

public sealed class PaperTradingPersistenceService(
    StockTraderDbContext dbContext) : IPaperTradingPersistence
{
    public async Task<PaperPortfolio?> GetPortfolioAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PaperPortfolios
            .Include(x => x.Positions)
            .Include(x => x.Trades)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaperPosition?> GetPositionAsync(Guid portfolioId, string symbol, CancellationToken cancellationToken = default)
    {
        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        return await dbContext.PaperPositions.FirstOrDefaultAsync(
                x =>
                    x.PaperPortfolioId == portfolioId &&
                    x.Symbol == normalizedSymbol,
                cancellationToken);
    }

    public async Task AddPortfolioAsync(PaperPortfolio portfolio, CancellationToken cancellationToken = default)
    {
        await dbContext.PaperPortfolios.AddAsync(
            portfolio,
            cancellationToken);
    }

    public async Task AddTradeAsync(PaperTrade trade, CancellationToken cancellationToken = default)
    {
        await dbContext.PaperTrades.AddAsync(
            trade,
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
