using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Infrastructure.Clients.SmartApi.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Infrastructure.Utilities;
using StockTrader.Shared.Results;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StockTrader.Infrastructure.Clients.SmartApi;

/// <summary>
/// Holds the current SmartAPI jwtToken/refreshToken in memory for the life of the
/// process (registered as a singleton, same lifetime as KiteSessionProvider and
/// ConversationMemory). A restart just triggers a fresh TOTP login on next use - no
/// manual step involved, unlike Zerodha.
/// </summary>
public sealed class SmartApiSessionProvider : ISmartApiSessionProvider
{
    // Angel One's own JWT is valid for longer than this, but refreshing well before
    // expiry avoids ever handing out a token that's about to be rejected mid-call.
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(6);

    private readonly HttpClient _httpClient;
    private readonly AngelOneOptions _options;
    private readonly ILogger<SmartApiSessionProvider> _logger;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    private string? _jwtToken;
    private string? _refreshToken;
    private DateTime _issuedAtUtc;

    public SmartApiSessionProvider(
        HttpClient httpClient,
        IOptions<AngelOneOptions> options,
        ILogger<SmartApiSessionProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<Result<string>> GetJwtTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_jwtToken is not null && DateTime.UtcNow - _issuedAtUtc < TokenLifetime)
            return Result<string>.Success(_jwtToken);

        await _loginLock.WaitAsync(cancellationToken);

        try
        {
            // Another caller may have refreshed while this one was waiting on the lock.
            if (_jwtToken is not null && DateTime.UtcNow - _issuedAtUtc < TokenLifetime)
                return Result<string>.Success(_jwtToken);

            if (_refreshToken is not null)
            {
                Result<string> refreshResult = await RefreshAsync(cancellationToken);

                if (refreshResult.IsSuccess)
                    return refreshResult;

                _logger.LogWarning("Angel One token refresh failed, falling back to a fresh login: {Error}", refreshResult.Error.Message);
            }

            return await LoginAsync(cancellationToken);
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private async Task<Result<string>> LoginAsync(CancellationToken cancellationToken)
    {
        try
        {
            string totpCode = TotpGenerator.GenerateCode(_options.TotpSecret);

            using HttpRequestMessage request = new(HttpMethod.Post, "rest/auth/angelbroking/user/v1/loginByPassword")
            {
                Content = JsonContent.Create(new
                {
                    clientcode = _options.ClientCode,
                    password = _options.Password,
                    totp = totpCode
                })
            };

            AddCommonHeaders(request);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await HandleLoginResponseAsync(response, "log in", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<string>.Failure(new Error("HttpRequestError", $"Unable to reach Angel One to log in: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("Exception", $"An error occurred while logging in to Angel One: {ex.Message}"));
        }
    }

    private async Task<Result<string>> RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Post, "rest/auth/angelbroking/jwt/v1/generateTokens")
            {
                Content = JsonContent.Create(new { refreshToken = _refreshToken })
            };

            AddCommonHeaders(request);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            return await HandleLoginResponseAsync(response, "refresh the session", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<string>.Failure(new Error("HttpRequestError", $"Unable to reach Angel One to refresh the session: {ex.Message}"));
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("Exception", $"An error occurred while refreshing the Angel One session: {ex.Message}"));
        }
    }

    private async Task<Result<string>> HandleLoginResponseAsync(HttpResponseMessage response, string action, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Angel One {Action} failed with {StatusCode}: {Body}", action, (int)response.StatusCode, body);

            return Result<string>.Failure(new Error("BrokerError", $"Angel One rejected the request to {action}: {body}"));
        }

        SmartApiResponse<SmartApiLoginData>? loginResponse = System.Text.Json.JsonSerializer.Deserialize<SmartApiResponse<SmartApiLoginData>>(body);

        if (loginResponse is null || !loginResponse.Status || string.IsNullOrWhiteSpace(loginResponse.Data?.JwtToken))
            return Result<string>.Failure(new Error("BrokerError", loginResponse?.Message ?? $"Angel One returned an empty response trying to {action}."));

        _jwtToken = loginResponse.Data.JwtToken;
        _refreshToken = loginResponse.Data.RefreshToken ?? _refreshToken;
        _issuedAtUtc = DateTime.UtcNow;

        return Result<string>.Success(_jwtToken);
    }

    private void AddCommonHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-UserType", "USER");
        request.Headers.Add("X-SourceID", "WEB");
        request.Headers.Add("X-ClientLocalIP", "127.0.0.1");
        request.Headers.Add("X-ClientPublicIP", "127.0.0.1");
        request.Headers.Add("X-MACAddress", _options.LocalMacAddress);
        request.Headers.Add("X-PrivateKey", _options.ApiKey);
    }
}
