using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using StockTrader.AI.Options;

namespace StockTrader.AI.Kernel;

public class KernelFactory(IOptions<AIOptions> options) : IKernelFactory
{
    private readonly AIOptions options = options.Value;

    public IKernelBuilder Create()
    {
        IKernelBuilder builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(modelId: options.Model, apiKey: options.ApiKey);

        return builder;
    }
}
