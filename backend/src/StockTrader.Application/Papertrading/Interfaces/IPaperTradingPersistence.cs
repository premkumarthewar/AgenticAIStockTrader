using StockTrader.Domain.Entities;

namespace StockTrader.Application.PaperTrading.Interfaces;

public interface IPaperTradingPersistence
{
    Task<PaperPortfolio?> GetPortfolioAsync(CancellationToken cancellationToken = default);

    Task<PaperPosition?> GetPositionAsync(Guid portfolioId, string symbol, CancellationToken cancellationToken = default);

    Task AddPortfolioAsync(PaperPortfolio portfolio, CancellationToken cancellationToken = default);

    Task AddTradeAsync(PaperTrade trade, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
