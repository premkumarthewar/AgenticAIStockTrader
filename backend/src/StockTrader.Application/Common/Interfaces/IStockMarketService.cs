using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

public interface IStockMarketService
{
    Task<Result<CompanyProfileDto>> GetCompanyProfileAsync(string symbol, CancellationToken cancellationToken = default);

    Task<Result<StockQuoteDto>> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<HistoricalPriceDto>>> GetHistoricalPricesAsync(string symbol, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<Result<FinancialsDto>> GetFinancialsAsync(string symbol, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NewsArticleDto>>> GetNewsAsync(string symbol, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
