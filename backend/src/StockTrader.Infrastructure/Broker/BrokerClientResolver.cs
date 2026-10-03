using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Broker;

/// <summary>
/// Resolves a keyed IBrokerClient by name. Each broker is registered in
/// DependencyInjection.cs under a key matching its BrokerProvider enum name exactly
/// ("Alpaca", "Zerodha", "AngelOne") - TradeApprovalDto.BrokerProvider is always that
/// same enum's ToString(), so the key always matches by construction.
/// </summary>
public sealed class BrokerClientResolver(IServiceProvider serviceProvider) : IBrokerClientResolver
{
    public Result<IBrokerClient> Resolve(string brokerProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brokerProvider);

        IBrokerClient? client = serviceProvider.GetKeyedService<IBrokerClient>(brokerProvider);

        if (client is null)
            return Result<IBrokerClient>.Failure(new Error(
                "BrokerNotConfigured",
                $"No broker is registered for provider '{brokerProvider}'."));

        return Result<IBrokerClient>.Success(client);
    }
}
