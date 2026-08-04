using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

public interface IStockMarketService
{
    Task<Result<CompanyProfileDto>> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
