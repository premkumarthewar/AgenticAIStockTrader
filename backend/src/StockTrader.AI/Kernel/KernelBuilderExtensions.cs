using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using StockTrader.AI.Options;

namespace StockTrader.AI.Kernel;

public static class KernelBuilderExtensions
{
    public static IKernelBuilder AddStockTraderKernel(this IKernelBuilder builder, IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(provider);

        AIOptions options = provider.GetRequiredService<IOptions<AIOptions>>().Value;

        builder.AddOpenAIChatCompletion(modelId: options.Model, apiKey: options.ApiKey);

        return builder;
    }
}
