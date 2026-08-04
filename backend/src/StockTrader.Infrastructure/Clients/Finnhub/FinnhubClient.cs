using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.Finnhub.Models;
using StockTrader.Infrastructure.Options;

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

        using var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FinnhubCompanyProfileResponse>(
            cancellationToken: cancellationToken);
    }
}