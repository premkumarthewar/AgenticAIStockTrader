using StockTrader.Domain.Entities;

namespace StockTrader.Application.Approvals.Interfaces;

public interface ITradeApprovalPersistence
{
    Task<TradeApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TradeApproval>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TradeApproval>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TradeApproval approval, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
