using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Persistence.Context;
using StockTrader.Persistence.Services;

namespace StockTrader.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockTraderDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IMemoryService, MemoryService>();

        services.AddScoped<IPaperTradingPersistence, PaperTradingPersistenceService>();

        return services;
    }
}
