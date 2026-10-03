using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Broker;
using StockTrader.Shared.Results;

namespace StockTrader.UnitTests.Infrastructure;

/// <summary>
/// Exercises BrokerClientResolver against a real ServiceCollection/ServiceProvider
/// (rather than mocking IServiceProvider) since the behavior under test - GetKeyedService
/// finding a match by string key - is exactly what keyed DI registration provides, and a
/// hand-rolled mock of IServiceProvider's keyed-resolution semantics would risk testing
/// the mock instead of the real thing.
/// </summary>
public class BrokerClientResolverTests
{
    // A do-nothing IBrokerClient - only its identity/type matters to these tests, never
    // its behavior, so every member throws.
    private sealed class FakeBrokerClient : IBrokerClient
    {
        public Task<Result<BrokerOrderDto>> PlaceOrderAsync(BrokerOrderRequestDto request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<BrokerOrderDto>> GetOrderAsync(string brokerOrderId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<BrokerAccountDto>> GetAccountAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private static IServiceProvider BuildProviderWithOneKeyedBroker(string key)
    {
        ServiceCollection services = new();

        services.AddKeyedSingleton<IBrokerClient, FakeBrokerClient>(key);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Resolve_WithRegisteredKey_ReturnsSuccessWithTheKeyedInstance()
    {
        IServiceProvider provider = BuildProviderWithOneKeyedBroker("Alpaca");

        BrokerClientResolver resolver = new(provider);

        Result<IBrokerClient> result = resolver.Resolve("Alpaca");

        Assert.True(result.IsSuccess);
        Assert.IsType<FakeBrokerClient>(result.Value);
    }

    [Fact]
    public void Resolve_WithUnregisteredKey_ReturnsBrokerNotConfiguredFailure()
    {
        IServiceProvider provider = BuildProviderWithOneKeyedBroker("Alpaca");

        BrokerClientResolver resolver = new(provider);

        Result<IBrokerClient> result = resolver.Resolve("Zerodha");

        Assert.True(result.IsFailure);
        Assert.Equal("BrokerNotConfigured", result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithNullOrWhitespaceKey_Throws(string brokerProvider)
    {
        IServiceProvider provider = BuildProviderWithOneKeyedBroker("Alpaca");

        BrokerClientResolver resolver = new(provider);

        Assert.Throws<ArgumentException>(() => resolver.Resolve(brokerProvider));
    }
}
