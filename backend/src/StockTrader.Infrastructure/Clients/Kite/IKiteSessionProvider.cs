using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.Kite;

/// <summary>
/// Kite Connect's login is interactive by design: a person has to visit GetLoginUrl(),
/// sign in on Zerodha's own page, and Zerodha redirects back with a one-time
/// request_token that ExchangeRequestTokenAsync trades for the day's access_token. There
/// is no fully unattended way around this (unlike Angel One's TOTP login) - a fresh
/// access_token is required once per trading day.
/// </summary>
public interface IKiteSessionProvider
{
    string GetLoginUrl();

    bool TryGetAccessToken(out string accessToken);

    Task<Result> ExchangeRequestTokenAsync(string requestToken, CancellationToken cancellationToken = default);
}
