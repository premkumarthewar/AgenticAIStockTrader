using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.SmartApi.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Net.Http.Json;

namespace StockTrader.Infrastructure.Clients.SmartApi;

/// <summary>
/// Downloads and caches Angel One's published scrip master (a several-MB JSON file
/// covering every instrument on every exchange) rather than fetching it per order.
/// Registered as a singleton (see DependencyInjection.cs) so the cache is shared across
/// requests; refreshed once a day since new listings/token changes are infrequent.
/// </summary>
public sealed class AngelOneInstrumentLookup : IAngelOneInstrumentLookup
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);

    private readonly HttpClient _httpClient;
    private readonly AngelOneOptions _options;
    private readonly ILogger<AngelOneInstrumentLookup> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private Dictionary<(string Exchange, string TradingSymbol), string>? _tokensBySymbol;
    private DateTime _cachedAtUtc;

    public AngelOneInstrumentLookup(
        HttpClient httpClient,
        IOptions<AngelOneOptions> options,
        ILogger<AngelOneInstrumentLookup> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<string>> GetSymbolTokenAsync(string symbol, string exchange, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange);

        Result ensureResult = await EnsureCacheAsync(cancellationToken);

        if (ensureResult.IsFailure)
            return Result<string>.Failure(ensureResult.Error);

        string normalizedExchange = exchange.Trim().ToUpperInvariant();

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        // NSE/BSE cash-market equities are listed with an "-EQ" suffix in Angel One's
        // scrip master (e.g. "INFY-EQ"); fall back to the bare symbol in case the caller
        // already supplied the fully-qualified trading symbol.
        string[] candidates = [$"{normalizedSymbol}-EQ", normalizedSymbol];

        foreach (string candidate in candidates)
        {
            if (_tokensBySymbol!.TryGetValue((normalizedExchange, candidate), out string? token))
                return Result<string>.Success(token);
        }

        return Result<string>.Failure(new Error(
            "NotFound",
            $"No Angel One instrument token was found for '{normalizedSymbol}' on {normalizedExchange}."));
    }

    private async Task<Result> EnsureCacheAsync(CancellationToken cancellationToken)
    {
        if (_tokensBySymbol is not null && DateTime.UtcNow - _cachedAtUtc < CacheLifetime)
            return Result.Success();

        await _refreshLock.WaitAsync(cancellationToken);

        try
        {
            if (_tokensBySymbol is not null && DateTime.UtcNow - _cachedAtUtc < CacheLifetime)
                return Result.Success();

            _logger.LogInformation("Refreshing the Angel One instrument/scrip master cache from {Url}", _options.ScripMasterUrl);

            List<SmartApiScripMasterEntry>? entries = await _httpClient.GetFromJsonAsync<List<SmartApiScripMasterEntry>>(
                _options.ScripMasterUrl, cancellationToken);

            if (entries is null || entries.Count == 0)
                return Result.Failure(new Error("BrokerError", "Angel One's instrument master returned no entries."));

            Dictionary<(string, string), string> tokensBySymbol = [];

            foreach (SmartApiScripMasterEntry entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Token) || string.IsNullOrWhiteSpace(entry.Symbol) || string.IsNullOrWhiteSpace(entry.ExchangeSegment))
                    continue;

                // Later entries win on a collision - the file has no duplicates in
                // practice, but this keeps the load defensive either way.
                tokensBySymbol[(entry.ExchangeSegment.Trim().ToUpperInvariant(), entry.Symbol.Trim().ToUpperInvariant())] = entry.Token;
            }

            _tokensBySymbol = tokensBySymbol;
            _cachedAtUtc = DateTime.UtcNow;

            _logger.LogInformation("Cached {Count} Angel One instrument tokens.", tokensBySymbol.Count);

            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(new Error("HttpRequestError", $"Unable to download Angel One's instrument master: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Exception", $"An error occurred while loading Angel One's instrument master: {ex.Message}"));
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}
