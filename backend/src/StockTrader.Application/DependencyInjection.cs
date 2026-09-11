using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Common.Interfaces;

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
