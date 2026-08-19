using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Net.Http.Json;

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

        using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

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

    public async Task<Result<FinancialsDto>> GetFinancialsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        try
        {
            string requestUri = $"stock/metric" + $"?symbol={Uri.EscapeDataString(normalizedSymbol)}" + $"&metric=all" + $"&token={_options.ApiKey}";

            FinnhubMetricsResponse? response =
            await _httpClient.GetFromJsonAsync<FinnhubMetricsResponse>(requestUri, cancellationToken);

            if (response?.Metric is null)
                return Result<FinancialsDto>.Failure(new Error("NotFound", $"Finnhub returned no financial information for {normalizedSymbol}"));

            FinancialsDto financials = new()
            {
                Symbol = normalizedSymbol,
                EarningsPerShare = response.Metric.EpsAnnual,
                GrossMargin = response.Metric.GrossMarginAnnual,
                OperatingMargin = response.Metric.OperatingMarginAnnual,
                NetMargin = response.Metric.NetProfitMarginAnnual,
                ReturnOnEquity = response.Metric.NetProfitMarginAnnual,
                ReturnOnAssets = response.Metric.RoaAnnual,
                DebtToEquity = response.Metric.TotalDebtToEquityAnnual
            };

            return Result<FinancialsDto>.Success(financials);
        }
        catch (HttpRequestException ex)
        {
            return Result<FinancialsDto>.Failure(new Error("InternalServerError", $"Unable to retrieve financial information for {normalizedSymbol}: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<FinancialsDto>.Failure(new Error("InternalServerError", $"An error occurred while retrieving financial information for {normalizedSymbol}: {ex.Message}"));
        }
    }

    public async Task<Result<IReadOnlyList<NewsArticleDto>>> GetNewsAsync(string symbol, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);

        if (from > to)
            return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("InvalidDateRange", "Start date cannot be greater than End date"));

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        try
        {
            string requestUri = $"company-news" + $"?symbol={Uri.EscapeDataString(normalizedSymbol)}" + $"&from={from}" + $"&to={to}" + $"&token={_options.ApiKey}";

            List<FinnhubNewsResponse>? response = await _httpClient.GetFromJsonAsync<List<FinnhubNewsResponse>>(requestUri, cancellationToken);

            if (response is null)
                return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("NotFound", $"Finnhub returned no news for {normalizedSymbol}"));

            List<NewsArticleDto> news = [.. response.Select(article => new NewsArticleDto
            {
                Headline = article.Headline ?? string.Empty,
                Summary = article.Summary,
                Source = article.Source,
                Url = article.Url,
                PublishedAt = article.Datetime > 0 ? DateTimeOffset.FromUnixTimeSeconds(article.Datetime).UtcDateTime : DateTime.MinValue,
                Category = article.Category,
                RelatedSymbol = normalizedSymbol
            }).Where(article => !string.IsNullOrEmpty(article.Headline))];

            return Result<IReadOnlyList<NewsArticleDto>>.Success(news);
        }
        catch (HttpRequestException ex)
        {
            return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("InternalServerError", $"Unable to retrieve news for {normalizedSymbol}: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<NewsArticleDto>>.Failure(new Error("InternalServerError", $"An error occurred while retrieving news for {normalizedSymbol}: {ex.Message}"));
        }
    }
}