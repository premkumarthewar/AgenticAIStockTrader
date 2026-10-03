using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.Kite.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Globalization;
using System.Text.Json;

namespace StockTrader.Infrastructure.Clients.Kite;

/// <summary>
/// Authenticated Kite Connect v3 calls. Every request here needs the day's access_token
/// from IKiteSessionProvider - if no session is active yet (or it expired overnight),
/// every method fails fast with a clear "log in" error rather than sending an
/// unauthenticated request Kite would just reject anyway.
/// </summary>
public sealed class KiteConnectClient : IKiteConnectClient
{
    private readonly HttpClient _httpClient;
    private readonly IKiteSessionProvider _sessionProvider;
    private readonly ILogger<KiteConnectClient> _logger;
    private readonly ZerodhaOptions _options;

    public KiteConnectClient(
        HttpClient httpClient,
        IKiteSessionProvider sessionProvider,
        IOptions<ZerodhaOptions> options,
        ILogger<KiteConnectClient> logger)
    {
        _httpClient = httpClient;
        _sessionProvider = sessionProvider;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/");
    }

    public async Task<Result<KiteOrderData>> PlaceOrderAsync(
        string tradingSymbol,
        string exchange,
        string transactionType,
        string orderType,
        decimal quantity,
        string product,
        decimal? price,
        CancellationToken cancellationToken = default)
    {
        if (!TryCreateAuthorizedRequest(HttpMethod.Post, "orders/regular", out HttpRequestMessage request, out Result<KiteOrderData> authFailure))
            return authFailure;

        List<KeyValuePair<string, string>> fields =
        [
            new("tradingsymbol", tradingSymbol),
            new("exchange", exchange),
            new("transaction_type", transactionType),
            new("order_type", orderType),
            new("quantity", quantity.ToString(CultureInfo.InvariantCulture)),
            new("product", product),
            new("validity", "DAY")
        ];

        if (price.HasValue && price.Value > 0)
            fields.Add(new("price", price.Value.ToString(CultureInfo.InvariantCulture)));

        request.Content = new FormUrlEncodedContent(fields);

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await ReadEnvelopeAsync<KiteOrderData>(response, "place order", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<KiteOrderData>.Failure(new Error("HttpRequestError", $"Unable to reach Zerodha to place the order: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<KiteOrderData>.Failure(new Error("Exception", $"An error occurred while placing the order with Zerodha: {ex.Message}"));
        }
    }

    public async Task<Result<KiteOrderStatus>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);

        if (!TryCreateAuthorizedRequest(HttpMethod.Get, $"orders/{Uri.EscapeDataString(orderId)}", out HttpRequestMessage request, out Result<KiteOrderStatus> authFailure))
            return authFailure;

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            Result<List<KiteOrderStatus>> historyResult = await ReadEnvelopeAsync<List<KiteOrderStatus>>(response, "retrieve order", cancellationToken);

            if (historyResult.IsFailure)
                return Result<KiteOrderStatus>.Failure(historyResult.Error);

            KiteOrderStatus? latest = historyResult.Value.Count > 0 ? historyResult.Value[^1] : null;

            if (latest is null)
                return Result<KiteOrderStatus>.Failure(new Error("NotFound", $"Zerodha returned no status history for order {orderId}."));

            return Result<KiteOrderStatus>.Success(latest);
        }
        catch (HttpRequestException ex)
        {
            return Result<KiteOrderStatus>.Failure(new Error("HttpRequestError", $"Unable to retrieve order {orderId} from Zerodha: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<KiteOrderStatus>.Failure(new Error("Exception", $"An error occurred while retrieving order {orderId} from Zerodha: {ex.Message}"));
        }
    }

    public async Task<Result<KiteMarginsData>> GetMarginsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateAuthorizedRequest(HttpMethod.Get, "user/margins", out HttpRequestMessage request, out Result<KiteMarginsData> authFailure))
            return authFailure;

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await ReadEnvelopeAsync<KiteMarginsData>(response, "retrieve margins", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<KiteMarginsData>.Failure(new Error("HttpRequestError", $"Unable to retrieve margins from Zerodha: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<KiteMarginsData>.Failure(new Error("Exception", $"An error occurred while retrieving margins from Zerodha: {ex.Message}"));
        }
    }

    private bool TryCreateAuthorizedRequest<T>(HttpMethod method, string path, out HttpRequestMessage request, out Result<T> authFailure)
    {
        if (!_sessionProvider.TryGetAccessToken(out string accessToken))
        {
            request = null!;

            authFailure = Result<T>.Failure(new Error(
                "BrokerSessionExpired",
                $"No active Zerodha session. Visit {_sessionProvider.GetLoginUrl()} to log in for today before retrying."));

            return false;
        }

        request = new HttpRequestMessage(method, path);

        request.Headers.Add("Authorization", $"token {_options.ApiKey}:{accessToken}");

        request.Headers.Add("X-Kite-Version", "3");

        authFailure = default!;

        return true;
    }

    private async Task<Result<T>> ReadEnvelopeAsync<T>(HttpResponseMessage response, string action, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        KiteApiResponse<T>? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<KiteApiResponse<T>>(body);
        }
        catch (JsonException)
        {
            envelope = null;
        }

        if (!response.IsSuccessStatusCode || envelope is null || !envelope.IsSuccess)
        {
            _logger.LogWarning("Zerodha {Action} failed with {StatusCode}: {Body}", action, (int)response.StatusCode, body);

            string message = envelope?.Message ?? (string.IsNullOrWhiteSpace(body) ? $"Zerodha returned {(int)response.StatusCode} with no error body." : body);

            return Result<T>.Failure(new Error(envelope?.ErrorType ?? "BrokerError", message));
        }

        if (envelope.Data is null)
            return Result<T>.Failure(new Error("NotFound", $"Zerodha returned an empty response for {action}."));

        return Result<T>.Success(envelope.Data);
    }
}
