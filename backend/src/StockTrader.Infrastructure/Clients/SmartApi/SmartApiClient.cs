using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.SmartApi.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StockTrader.Infrastructure.Clients.SmartApi;

/// <summary>
/// Authenticated SmartAPI calls. Every request needs the current jwtToken from
/// ISmartApiSessionProvider, refreshed/re-issued internally - callers here never think
/// about login at all.
/// </summary>
public sealed class SmartApiClient : ISmartApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISmartApiSessionProvider _sessionProvider;
    private readonly AngelOneOptions _options;
    private readonly ILogger<SmartApiClient> _logger;

    public SmartApiClient(
        HttpClient httpClient,
        ISmartApiSessionProvider sessionProvider,
        IOptions<AngelOneOptions> options,
        ILogger<SmartApiClient> logger)
    {
        _httpClient = httpClient;
        _sessionProvider = sessionProvider;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<Result<SmartApiOrderData>> PlaceOrderAsync(
        string tradingSymbol,
        string symbolToken,
        string exchange,
        string transactionType,
        string orderType,
        string productType,
        decimal quantity,
        decimal? price,
        CancellationToken cancellationToken = default)
    {
        Result<HttpRequestMessage> requestResult = await CreateAuthorizedRequestAsync<SmartApiOrderData>(
            HttpMethod.Post, "rest/secure/angelbroking/order/v1/placeOrder", cancellationToken);

        if (requestResult.IsFailure)
            return Result<SmartApiOrderData>.Failure(requestResult.Error);

        using HttpRequestMessage request = requestResult.Value;

        request.Content = JsonContent.Create(new
        {
            variety = "NORMAL",
            tradingsymbol = tradingSymbol,
            symboltoken = symbolToken,
            transactiontype = transactionType,
            exchange,
            ordertype = orderType,
            producttype = productType,
            duration = "DAY",
            price = (price ?? 0m).ToString(CultureInfo.InvariantCulture),
            squareoff = "0",
            stoploss = "0",
            quantity = ((int)quantity).ToString(CultureInfo.InvariantCulture)
        });

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await ReadEnvelopeAsync<SmartApiOrderData>(response, "place order", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<SmartApiOrderData>.Failure(new Error("HttpRequestError", $"Unable to reach Angel One to place the order: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<SmartApiOrderData>.Failure(new Error("Exception", $"An error occurred while placing the order with Angel One: {ex.Message}"));
        }
    }

    public async Task<Result<SmartApiOrderBookEntry>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);

        Result<HttpRequestMessage> requestResult = await CreateAuthorizedRequestAsync<SmartApiOrderBookEntry>(
            HttpMethod.Get, "rest/secure/angelbroking/order/v1/getOrderBook", cancellationToken);

        if (requestResult.IsFailure)
            return Result<SmartApiOrderBookEntry>.Failure(requestResult.Error);

        using HttpRequestMessage request = requestResult.Value;

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            Result<List<SmartApiOrderBookEntry>> bookResult = await ReadEnvelopeAsync<List<SmartApiOrderBookEntry>>(response, "retrieve the order book", cancellationToken);

            if (bookResult.IsFailure)
                return Result<SmartApiOrderBookEntry>.Failure(bookResult.Error);

            SmartApiOrderBookEntry? order = bookResult.Value.FirstOrDefault(x => string.Equals(x.OrderId, orderId, StringComparison.OrdinalIgnoreCase));

            if (order is null)
                return Result<SmartApiOrderBookEntry>.Failure(new Error("NotFound", $"Order {orderId} was not found in Angel One's order book."));

            return Result<SmartApiOrderBookEntry>.Success(order);
        }
        catch (HttpRequestException ex)
        {
            return Result<SmartApiOrderBookEntry>.Failure(new Error("HttpRequestError", $"Unable to retrieve order {orderId} from Angel One: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<SmartApiOrderBookEntry>.Failure(new Error("Exception", $"An error occurred while retrieving order {orderId} from Angel One: {ex.Message}"));
        }
    }

    public async Task<Result<SmartApiRmsData>> GetRmsAsync(CancellationToken cancellationToken = default)
    {
        Result<HttpRequestMessage> requestResult = await CreateAuthorizedRequestAsync<SmartApiRmsData>(
            HttpMethod.Get, "rest/secure/angelbroking/user/v1/getRMS", cancellationToken);

        if (requestResult.IsFailure)
            return Result<SmartApiRmsData>.Failure(requestResult.Error);

        using HttpRequestMessage request = requestResult.Value;

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await ReadEnvelopeAsync<SmartApiRmsData>(response, "retrieve funds", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<SmartApiRmsData>.Failure(new Error("HttpRequestError", $"Unable to retrieve funds from Angel One: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<SmartApiRmsData>.Failure(new Error("Exception", $"An error occurred while retrieving funds from Angel One: {ex.Message}"));
        }
    }

    private async Task<Result<HttpRequestMessage>> CreateAuthorizedRequestAsync<T>(HttpMethod method, string path, CancellationToken cancellationToken)
    {
        Result<string> tokenResult = await _sessionProvider.GetJwtTokenAsync(cancellationToken);

        if (tokenResult.IsFailure)
            return Result<HttpRequestMessage>.Failure(tokenResult.Error);

        HttpRequestMessage request = new(method, path);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-UserType", "USER");
        request.Headers.Add("X-SourceID", "WEB");
        request.Headers.Add("X-ClientLocalIP", "127.0.0.1");
        request.Headers.Add("X-ClientPublicIP", "127.0.0.1");
        request.Headers.Add("X-MACAddress", _options.LocalMacAddress);
        request.Headers.Add("X-PrivateKey", _options.ApiKey);

        return Result<HttpRequestMessage>.Success(request);
    }

    private async Task<Result<T>> ReadEnvelopeAsync<T>(HttpResponseMessage response, string action, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        SmartApiResponse<T>? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<SmartApiResponse<T>>(body);
        }
        catch (JsonException)
        {
            envelope = null;
        }

        if (!response.IsSuccessStatusCode || envelope is null || !envelope.Status)
        {
            _logger.LogWarning("Angel One {Action} failed with {StatusCode}: {Body}", action, (int)response.StatusCode, body);

            string message = envelope?.Message ?? (string.IsNullOrWhiteSpace(body) ? $"Angel One returned {(int)response.StatusCode} with no error body." : body);

            return Result<T>.Failure(new Error(envelope?.ErrorCode is { Length: > 0 } code ? code : "BrokerError", message));
        }

        if (envelope.Data is null)
            return Result<T>.Failure(new Error("NotFound", $"Angel One returned an empty response for {action}."));

        return Result<T>.Success(envelope.Data);
    }
}
