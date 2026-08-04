using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StockTrader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        // Market API
        // Broker API
        // Email
        // Logging
        return services;
    }
}
