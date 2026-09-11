using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StockTrader.Persistence.Context;
using StockTrader.Application.Common.Interfaces;
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

        return services;
    }
}
