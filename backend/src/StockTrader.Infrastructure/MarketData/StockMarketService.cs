using Microsoft.Extensions.Logging;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Infrastructure.Clients.Finnhub;
using StockTrader.Infrastructure.Clients.Finnhub.Mappers;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.MarketData;

public sealed class StockMarketService(
    IFinnhubClient finnhubClient,
    ILogger<StockMarketService> logger) : IStockMarketService
{
    public async Task<Result<CompanyProfileDto>> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Getting company profile for {Symbol}",
            symbol);

        FinnhubCompanyProfileResponse? response = await finnhubClient.GetCompanyProfileAsync(
            symbol,
            cancellationToken);

        if (response is null || string.IsNullOrWhiteSpace(response.Ticker))
            return Result<CompanyProfileDto>.Failure(
                new Error("CompanyNotFound", "Company profile not found."));

        CompanyProfileDto dto = CompanyProfileMapper.Map(response);

        return Result<CompanyProfileDto>.Success(dto);
    }

    public async Task<Result<StockQuoteDto>> GetQuoteAsync(
    string symbol,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        return await finnhubClient.GetQuoteAsync(
            symbol,
            cancellationToken);
    }

    public async Task<Result<IReadOnlyList<HistoricalPriceDto>>> GetHistoricalPricesAsync(
    string symbol,
    DateTime from,
    DateTime to,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        if (from > to)
            return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(new Error("InvalidDateRange",
                "The start date cannot be later than the end date."));

        return await finnhubClient.GetHistoricalPricesAsync(
            symbol,
            from,
            to,
            cancellationToken);
    }

    public async Task<Result<FinancialsDto>> GetFinancialsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(symbol))
            return Result<FinancialsDto>.Failure(new Error("BadRequest", "Stock symbol is required"));

        return await finnhubClient.GetFinancialsAsync(symbol, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<NewsArticleDto>>> GetNewsAsync(string symbol, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(symbol))
            return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("BadRequest", "Stock symbol is required"));

        if (from > to)
            return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("InvalidDateRange", "Start date cannot be greater than end date"));

        return await finnhubClient.GetNewsAsync(symbol, from, to, cancellationToken);
    }
}
