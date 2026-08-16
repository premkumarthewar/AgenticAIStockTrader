using Microsoft.SemanticKernel;

namespace StockTrader.AI.Kernel;

public interface IKernelFactory
{
    IKernelBuilder Create();

    Microsoft.SemanticKernel.Kernel CreateKernel();
}
