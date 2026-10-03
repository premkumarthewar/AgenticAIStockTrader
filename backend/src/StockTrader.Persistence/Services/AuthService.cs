using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockTrader.Application.Auth.Dtos;
using StockTrader.Application.Auth.Interfaces;
using StockTrader.Persistence.Identity;
using StockTrader.Persistence.Options;
using StockTrader.Shared.Results;

namespace StockTrader.Persistence.Services;

/// <summary>
/// Registration/login backed by ASP.NET Core Identity's UserManager (password hashing,
/// lockout, uniqueness) plus hand-issued JWT bearer tokens (no separate identity
/// provider - the API is its own issuer). JwtOptions is bound once here and again by the
/// Api project's JwtBearer handler, from the same "Jwt" configuration section, so a token
/// this mints is always accepted by this same API's own validation.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<AuthResultDto>.Failure(new Error("BadRequest", "Email is required."));

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResultDto>.Failure(new Error("BadRequest", "Password is required."));

        ApplicationUser? existing = await userManager.FindByEmailAsync(request.Email);

        if (existing is not null)
            return Result<AuthResultDto>.Failure(new Error("Conflict", "A user with this email already exists."));

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedOnUtc = DateTime.UtcNow
        };

        IdentityResult createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            string message = string.Join(" ", createResult.Errors.Select(x => x.Description));

            return Result<AuthResultDto>.Failure(new Error("BadRequest", message));
        }

        return Result<AuthResultDto>.Success(CreateToken(user));
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResultDto>.Failure(new Error("Unauthorized", "Invalid email or password."));

        ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result<AuthResultDto>.Failure(new Error("Unauthorized", "Invalid email or password."));

        bool passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
            return Result<AuthResultDto>.Failure(new Error("Unauthorized", "Invalid email or password."));

        return Result<AuthResultDto>.Success(CreateToken(user));
    }

    private AuthResultDto CreateToken(ApplicationUser user)
    {
        DateTime expiresOnUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        SigningCredentials credentials = new(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresOnUtc,
            signingCredentials: credentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResultDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            AccessToken = accessToken,
            ExpiresOnUtc = expiresOnUtc
        };
    }
}
