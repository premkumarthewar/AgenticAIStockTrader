using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.Auth.Dtos;
using StockTrader.Application.Auth.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

/// <summary>
/// Registration and login. The only controller in the API that doesn't require a bearer
/// token, since its whole job is to issue one.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        Result<AuthResultDto> result = await authService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResultDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        Result<AuthResultDto> result = await authService.LoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok(result.Value);
    }
}
