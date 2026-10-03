namespace StockTrader.Application.Auth.Dtos;

public sealed record AuthResultDto
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string AccessToken { get; init; }

    public required DateTime ExpiresOnUtc { get; init; }
}
