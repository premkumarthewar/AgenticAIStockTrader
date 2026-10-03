using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.Alpaca.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockTrader.Infrastructure.Clients.Alpaca;

/// <summary>
/// Thin wrapper over Alpaca's Trading API (https://docs.alpaca.markets/reference).
/// Authenticates with the APCA-API-KEY-ID / APCA-API-SECRET-KEY headers rather than a
/// query-string token, unlike FinnhubClient. BaseUrl (and therefore whether this hits
/// Alpaca's paper or live endpoint) comes entirely from AlpacaOptions.
/// </summary>
public sealed class AlpacaClient : IAlpacaClient
{
    private const string OrdersPath = "v2/orders";
    private const string AccountPath = "v2/account";

    private readonly HttpClient _httpClient;
    private readonly ILogger<AlpacaClient> _logger;
    private readonly AlpacaOptions _options;

    public AlpacaClient(
        HttpClient httpClient,
        IOptions<AlpacaOptions> options,
        ILogger<AlpacaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _options.ApiKeyId);
        _httpClient.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _options.ApiSecretKey);
    }

    public async Task<Result<AlpacaOrderResponse>> PlaceOrderAsync(AlpacaOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            _logger.LogInformation(
                "Submitting {Side} order for {Quantity} {Symbol} to Alpaca ({Mode})",
                request.Side,
                request.Quantity,
                request.Symbol,
                _options.IsPaperTrading ? "paper" : "live");

            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(OrdersPath, request, cancellationToken);

            return await ReadResultAsync(response, "place order", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<AlpacaOrderResponse>.Failure(new Error("HttpRequestError", $"Unable to reach Alpaca to place the order: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<AlpacaOrderResponse>.Failure(new Error("Exception", $"An error occurred while placing the order with Alpaca: {ex.Message}"));
        }
    }

    public async Task<Result<AlpacaOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"{OrdersPath}/{Uri.EscapeDataString(orderId)}", cancellationToken);

            return await ReadResultAsync(response, "retrieve order", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<AlpacaOrderResponse>.Failure(new Error("HttpRequestError", $"Unable to retrieve order {orderId} from Alpaca: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<AlpacaOrderResponse>.Failure(new Error("Exception", $"An error occurred while retrieving order {orderId} from Alpaca: {ex.Message}"));
        }
    }

    public async Task<Result<AlpacaAccountResponse>> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(AccountPath, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogWarning("Alpaca account lookup failed with {StatusCode}: {Body}", (int)response.StatusCode, errorBody);

                return Result<AlpacaAccountResponse>.Failure(new Error("BrokerError", ExtractMessage(errorBody, response.StatusCode)));
            }

            AlpacaAccountResponse? account = await response.Content.ReadFromJsonAsync<AlpacaAccountResponse>(cancellationToken: cancellationToken);

            if (account is null)
                return Result<AlpacaAccountResponse>.Failure(new Error("NotFound", "Alpaca returned an empty account response."));

            return Result<AlpacaAccountResponse>.Success(account);
        }
        catch (HttpRequestException ex)
        {
            return Result<AlpacaAccountResponse>.Failure(new Error("HttpRequestError", $"Unable to retrieve the Alpaca account: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<AlpacaAccountResponse>.Failure(new Error("Exception", $"An error occurred while retrieving the Alpaca account: {ex.Message}"));
        }
    }

    private async Task<Result<AlpacaOrderResponse>> ReadResultAsync(HttpResponseMessage response, string action, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning("Alpaca {Action} failed with {StatusCode}: {Body}", action, (int)response.StatusCode, errorBody);

            return Result<AlpacaOrderResponse>.Failure(new Error("BrokerError", ExtractMessage(errorBody, response.StatusCode)));
        }

        AlpacaOrderResponse? order = await response.Content.ReadFromJsonAsync<AlpacaOrderResponse>(cancellationToken: cancellationToken);

        if (order is null)
            return Result<AlpacaOrderResponse>.Failure(new Error("NotFound", $"Alpaca returned an empty response for {action}."));

        return Result<AlpacaOrderResponse>.Success(order);
    }

    private static string ExtractMessage(string errorBody, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(errorBody))
            return $"Alpaca returned {(int)statusCode} {statusCode} with no error body.";

        try
        {
            AlpacaOrderResponse? parsed = System.Text.Json.JsonSerializer.Deserialize<AlpacaOrderResponse>(errorBody);

            if (!string.IsNullOrWhiteSpace(parsed?.Message))
                return parsed.Message;
        }
        catch (System.Text.Json.JsonException)
        {
            // Fall through and surface the raw body below.
        }

        return errorBody;
    }
}
