using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Plugins;

public sealed class CompanyProfilePlugin(IStockMarketService service)
{
    public async Task<Result<CompanyProfileDto>> GetCompanyProfileAsync(string symbol)
    {
        return await service.GetCompanyProfileAsync(symbol);
    }
}
