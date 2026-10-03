namespace StockTrader.Application.Auth.Dtos;

public sealed record RegisterRequestDto
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public string? DisplayName { get; init; }
}
