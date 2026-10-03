namespace StockTrader.Application.Common.Interfaces;

/// <summary>
/// Reads the authenticated caller's identity out of the current HTTP request. Implemented
/// in the Api project (via IHttpContextAccessor) since that's the only layer that knows
/// about HttpContext; every other layer (paper trading, approvals) depends only on this
/// abstraction so a userId can be threaded through without any layer but Api touching
/// ClaimsPrincipal directly.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    /// <summary>
    /// The authenticated user's id. Throws InvalidOperationException if called with no
    /// authenticated user - only safe to read behind an [Authorize] endpoint, which is
    /// every place this is currently used.
    /// </summary>
    Guid UserId { get; }

    string? Email { get; }
}
