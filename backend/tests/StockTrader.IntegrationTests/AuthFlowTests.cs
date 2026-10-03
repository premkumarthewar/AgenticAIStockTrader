using System.Net;
using System.Net.Http.Json;
using StockTrader.Application.Auth.Dtos;

namespace StockTrader.IntegrationTests;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static RegisterRequestDto NewRegistration() => new()
    {
        Email = $"{Guid.NewGuid():N}@stocktrader.test",
        Password = "P@ssw0rd!23",
        DisplayName = "Integration Test User"
    };

    [Fact]
    public async Task Register_WithNewEmail_ReturnsAccessToken()
    {
        RegisterRequestDto request = NewRegistration();

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResultDto? result = await response.Content.ReadFromJsonAsync<AuthResultDto>();

        Assert.NotNull(result);
        Assert.Equal(request.Email, result!.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task Register_WithAnAlreadyRegisteredEmail_ReturnsBadRequest()
    {
        RegisterRequestDto request = NewRegistration();

        HttpResponseMessage first = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        HttpResponseMessage second = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsAccessToken()
    {
        RegisterRequestDto registration = NewRegistration();

        await _client.PostAsJsonAsync("/api/auth/register", registration);

        LoginRequestDto login = new() { Email = registration.Email, Password = registration.Password };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResultDto? result = await response.Content.ReadFromJsonAsync<AuthResultDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        RegisterRequestDto registration = NewRegistration();

        await _client.PostAsJsonAsync("/api/auth/register", registration);

        LoginRequestDto login = new() { Email = registration.Email, Password = "not-the-right-password" };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PaperTradingPortfolio_WithoutABearerToken_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/AI/paper-trading/portfolio");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
