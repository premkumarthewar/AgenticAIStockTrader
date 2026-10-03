using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.Kite.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Security.Cryptography;
using System.Text;

namespace StockTrader.Infrastructure.Clients.Kite;

/// <summary>
/// Holds the current Kite Connect access_token in memory for the life of the process
/// (registered as a singleton in DependencyInjection.cs, same lifetime as
/// ConversationMemory). A restart clears it and requires a fresh interactive login -
/// acceptable for now; promoting this to a persisted session is a later step if needed.
/// </summary>
public sealed class KiteSessionProvider : IKiteSessionProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KiteSessionProvider> _logger;
    private readonly ZerodhaOptions _options;
    private readonly object _lock = new();

    private string? _accessToken;
    private DateOnly? _issuedOnIst;

    public KiteSessionProvider(
        HttpClient httpClient,
        IOptions<ZerodhaOptions> options,
        ILogger<KiteSessionProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/");
    }

    public string GetLoginUrl()
    {
        return $"{_options.LoginBaseUrl}?v=3&api_key={Uri.EscapeDataString(_options.ApiKey)}";
    }

    public bool TryGetAccessToken(out string accessToken)
    {
        lock (_lock)
        {
            // Kite's access_token is valid until Zerodha's daily session reset
            // (~7:30am IST the next day), not a fixed 24h window. Comparing calendar
            // dates in IST is a close, simple approximation of that rule.
            if (_accessToken is not null && _issuedOnIst == TodayIst())
            {
                accessToken = _accessToken;
                return true;
            }

            accessToken = string.Empty;
            return false;
        }
    }

    public async Task<Result> ExchangeRequestTokenAsync(string requestToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestToken);

        try
        {
            string checksum = ComputeChecksum(_options.ApiKey, requestToken, _options.ApiSecret);

            using FormUrlEncodedContent content = new(
            [
                new KeyValuePair<string, string>("api_key", _options.ApiKey),
                new KeyValuePair<string, string>("request_token", requestToken),
                new KeyValuePair<string, string>("checksum", checksum)
            ]);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using HttpRequestMessage message = new(HttpMethod.Post, "session/token") { Content = content };

            message.Headers.Add("X-Kite-Version", "3");

            using HttpResponseMessage response = await _httpClient.SendAsync(message, cancellationToken);

            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Zerodha session exchange failed with {StatusCode}: {Body}", (int)response.StatusCode, body);

                return Result.Failure(new Error("BrokerError", $"Zerodha rejected the login: {body}"));
            }

            KiteApiResponse<KiteSessionData>? sessionResponse = System.Text.Json.JsonSerializer.Deserialize<KiteApiResponse<KiteSessionData>>(body);

            if (sessionResponse is null || !sessionResponse.IsSuccess || string.IsNullOrWhiteSpace(sessionResponse.Data?.AccessToken))
                return Result.Failure(new Error("BrokerError", sessionResponse?.Message ?? "Zerodha returned an empty session response."));

            lock (_lock)
            {
                _accessToken = sessionResponse.Data.AccessToken;
                _issuedOnIst = TodayIst();
            }

            _logger.LogInformation("Zerodha session established for user {UserId}", sessionResponse.Data.UserId);

            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(new Error("HttpRequestError", $"Unable to reach Zerodha to exchange the login token: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Exception", $"An error occurred while establishing the Zerodha session: {ex.Message}"));
        }
    }

    private static string ComputeChecksum(string apiKey, string requestToken, string apiSecret)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey + requestToken + apiSecret));

        return Convert.ToHexStringLower(hash);
    }

    private static DateOnly TodayIst()
    {
        TimeZoneInfo istZone = ResolveIstTimeZone();

        DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        return DateOnly.FromDateTime(istNow);
    }

    private static TimeZoneInfo ResolveIstTimeZone()
    {
        foreach (string id in new[] { "Asia/Kolkata", "India Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // Try the next known id.
            }
        }

        // IST has a fixed +05:30 offset with no DST, so a manual fallback is exact.
        return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "IST");
    }
}
