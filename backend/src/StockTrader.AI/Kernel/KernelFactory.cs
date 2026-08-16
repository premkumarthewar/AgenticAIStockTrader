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

    public Microsoft.SemanticKernel.Kernel CreateKernel()
    {
        if (string.IsNullOrEmpty(options.ApiKey))
            throw new InvalidOperationException("AI API key has not been configured");

        if (string.IsNullOrEmpty(options.Model))
            throw new InvalidOperationException("AI Model has not been configured");

        IKernelBuilder builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(modelId: options.Model, apiKey: options.ApiKey);

        return builder.Build();
    }
}
