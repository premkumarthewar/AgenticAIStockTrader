using Microsoft.EntityFrameworkCore;
using StockTrader.Application.Approvals.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Context;

namespace StockTrader.Persistence.Services;

public sealed class TradeApprovalPersistenceService(
    StockTraderDbContext dbContext) : ITradeApprovalPersistence
{
    public async Task<TradeApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.TradeApprovals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TradeApproval>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TradeApprovals
            .Where(x => x.Status == TradeApprovalStatus.Pending)
            .OrderBy(x => x.RequestedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TradeApproval>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TradeApprovals
            .OrderByDescending(x => x.RequestedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TradeApproval approval, CancellationToken cancellationToken = default)
    {
        await dbContext.TradeApprovals.AddAsync(approval, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
