using System.Security.Claims;
using StockTrader.Application.Common.Interfaces;

namespace StockTrader.Api.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            string? value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out Guid userId))
                throw new InvalidOperationException("No authenticated user is available on the current request.");

            return userId;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
}
