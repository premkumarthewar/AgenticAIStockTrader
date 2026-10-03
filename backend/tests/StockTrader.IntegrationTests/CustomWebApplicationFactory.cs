using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.Persistence.Context;

namespace StockTrader.IntegrationTests;

/// <summary>
/// Boots the real Api pipeline (auth, health checks, controllers, Serilog, OpenTelemetry)
/// against the "Testing" environment (appsettings.Testing.json - dummy AI/Finnhub keys,
/// no real broker credentials), with the one substitution every test here needs: the SQL
/// Server-backed StockTraderDbContext is swapped for a fresh, isolated EF Core InMemory
/// database per factory instance, so tests never depend on a reachable SQL Server.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"StockTraderTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? dbContextOptionsDescriptor = services.SingleOrDefault(
                x => x.ServiceType == typeof(DbContextOptions<StockTraderDbContext>));

            if (dbContextOptionsDescriptor is not null)
                services.Remove(dbContextOptionsDescriptor);

            services.AddDbContext<StockTraderDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
