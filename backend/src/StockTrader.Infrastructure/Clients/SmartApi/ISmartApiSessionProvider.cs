using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.SmartApi;

/// <summary>
/// Unlike Zerodha, Angel One's login is fully programmatic (client code + password/PIN +
/// TOTP), so this can log itself in and refresh itself with no manual step. Callers just
/// ask for a token; whether that meant reusing a cached one, refreshing, or a fresh
/// TOTP login happens internally.
/// </summary>
public interface ISmartApiSessionProvider
{
    Task<Result<string>> GetJwtTokenAsync(CancellationToken cancellationToken = default);
}
