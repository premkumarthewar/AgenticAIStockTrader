using Microsoft.Extensions.Logging;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Infrastructure.Clients.Finnhub;
using StockTrader.Infrastructure.Clients.Finnhub.Mappers;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.MarketData;

public sealed class StockMarketService : IStockMarketService
{
    private readonly IFinnhubClient _finnhubClient;
    private readonly ILogger<StockMarketService> _logger;

    public StockMarketService(
        IFinnhubClient finnhubClient,
        ILogger<StockMarketService> logger)
    {
        _finnhubClient = finnhubClient;
        _logger = logger;
    }

    public async Task<Result<CompanyProfileDto>> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting company profile for {Symbol}",
            symbol);

        FinnhubCompanyProfileResponse? response = await _finnhubClient.GetCompanyProfileAsync(
            symbol,
            cancellationToken);

        if (response is null || string.IsNullOrWhiteSpace(response.Ticker))
        {
            return Result<CompanyProfileDto>.Failure(
                new Error("CompanyNotFound", "Company profile not found."));
        }

        var dto = CompanyProfileMapper.Map(response);
        return Result<CompanyProfileDto>.Success(dto);
    }
}
