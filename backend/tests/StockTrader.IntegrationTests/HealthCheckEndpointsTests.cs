using System.Net;

namespace StockTrader.IntegrationTests;

public class HealthCheckEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_ReturnsOk_AndRequiresNoAuthentication()
    {
        HttpResponseMessage response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"status\"", body);
        Assert.Contains("Healthy", body);
    }
}
