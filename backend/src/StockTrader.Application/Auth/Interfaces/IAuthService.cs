using StockTrader.Application.Auth.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResultDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<AuthResultDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
