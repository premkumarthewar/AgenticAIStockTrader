using System.Linq.Expressions;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.Finnhub;

public sealed class FinnhubClient : IFinnhubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinnhubClient> _logger;
    private readonly FinnhubOptions _options;

    public FinnhubClient(
        HttpClient httpClient,
        IOptions<FinnhubOptions> options,
        ILogger<FinnhubClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<FinnhubCompanyProfileResponse?> GetCompanyProfileAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var requestUri =
            $"stock/profile2?symbol={Uri.EscapeDataString(symbol)}&token={_options.ApiKey}";

        _logger.LogInformation(
            "Retrieving company profile from Finnhub for symbol {Symbol}",
            symbol);

        using HttpResponseMessage response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FinnhubCompanyProfileResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<Result<StockQuoteDto>> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        try
        {
            string normalizedSymbol = symbol.Trim().ToUpperInvariant();

            FinnhubQuoteResponse? response = await _httpClient.GetFromJsonAsync<FinnhubQuoteResponse>($"quote?symbol={Uri.EscapeDataString(normalizedSymbol)}&token={_options.ApiKey}", cancellationToken);

            if (response is null)
                return Result<StockQuoteDto>.Failure(new Error("NotFound", $"Finnhub returned an empty response for {normalizedSymbol}"));

            if (response.CurrentPrice <= 0)
                return Result<StockQuoteDto>.Failure(new Error("InvalidPrice", $"Finnhub returned an invalid price for {normalizedSymbol}"));

            DateTime timeStamp = response.Timestamp > 0
                ? DateTimeOffset.FromUnixTimeSeconds(response.Timestamp).UtcDateTime
                : DateTime.UtcNow;

            StockQuoteDto stockQuoteDto = new()
            {
                Symbol = normalizedSymbol,
                CurrentPrice = response.CurrentPrice,
                Change = response.Change,
                PercentChange = response.PercentChange,
                High = response.High,
                Low = response.Low,
                Open = response.Open,
                PreviousClose = response.PreviousClose,
                Timestamp = timeStamp
            };

            return Result<StockQuoteDto>.Success(stockQuoteDto);
        }
        catch (HttpRequestException ex)
        {
            return Result<StockQuoteDto>.Failure(new Error("HttpRequestError", $"Unable to retrieve quote for {symbol}: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<StockQuoteDto>.Failure(new Error("Exception", $"An error occurred while retrieving quote for {symbol}: {ex.Message}"));
        }
    }

    public async Task<Result<IReadOnlyList<HistoricalPriceDto>>> GetHistoricalPricesAsync(string symbol, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        if (from > to)
            return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(new Error("InvalidDateRange", "The start date cannot be later than the end date."));

        try
        {
            string normalizedSymbol = symbol.Trim().ToUpperInvariant();

            long fromUnix = new DateTimeOffset(from).ToUnixTimeSeconds();

            long toUnix = new DateTimeOffset(to).ToUnixTimeSeconds();

            string requestUri = $"stock/candle?symbol={Uri.EscapeDataString(normalizedSymbol)}&resolution=D&from={fromUnix}&to={toUnix}&token={_options.ApiKey}";

            _logger.LogInformation(
                "Retrieving historical prices from Finnhub for symbol {Symbol} from {From} to {To}",
                normalizedSymbol,
                from,
                to);

            using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

            response.EnsureSuccessStatusCode();

            FinnhubCandleResponse? candleResponse = await response.Content.ReadFromJsonAsync<FinnhubCandleResponse>(cancellationToken: cancellationToken);

            if (candleResponse is null || candleResponse.Status != "ok")
                return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(new Error("NotFound", $"Finnhub returned an empty or invalid response for {normalizedSymbol}"));

            if (candleResponse.Timestamps is null || candleResponse.Open is null || candleResponse.High is null || candleResponse.Low is null || candleResponse.Close is null || candleResponse.Volume is null)
                return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(
                    new Error("IncompleteData", $"Finnhub returned incomplete historical price data for {normalizedSymbol}."));

            int count = new[]
        {
            candleResponse.Timestamps.Count,
            candleResponse.Open.Count,
            candleResponse.High.Count,
            candleResponse.Low.Count,
            candleResponse.Close.Count,
            candleResponse.Volume.Count
        }.Min();

            List<HistoricalPriceDto> historicalPrices = new(count);

            for (int i = 0; i < count; i++)
            {
                historicalPrices.Add(new HistoricalPriceDto
                {
                    Symbol = normalizedSymbol,
                    Open = candleResponse.Open[i],
                    High = candleResponse.High[i],
                    Low = candleResponse.Low[i],
                    Close = candleResponse.Close[i],
                    Volume = candleResponse.Volume[i]
                });
            }

            return Result<IReadOnlyList<HistoricalPriceDto>>.Success(historicalPrices);
        }
        catch (HttpRequestException ex)
        {
            return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(new Error("HttpRequestError", $"Unable to retrieve historical prices for {symbol}: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<HistoricalPriceDto>>.Failure(
            new Error("Exception", $"An error occurred while retrieving historical prices for {symbol}: {ex.Message}"));
        }
    }
}