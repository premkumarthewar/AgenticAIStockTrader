using Microsoft.Extensions.DependencyInjection;

namespace StockTrader.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        // FluentValidation
        // AutoMapper
        // Application Services
        return services;
    }
}
