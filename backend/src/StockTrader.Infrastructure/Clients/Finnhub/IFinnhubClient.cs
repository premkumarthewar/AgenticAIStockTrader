using StockTrader.Infrastructure.Clients.Finnhub.Models;

namespace StockTrader.Infrastructure.Clients.Finnhub;

public interface IFinnhubClient
{
    Task<FinnhubCompanyProfileResponse?> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}