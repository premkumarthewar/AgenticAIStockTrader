using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace StockTrader.AI.Agents.Base;

/// <summary>
/// Represents the runtime context for an AI agent.
/// </summary>
public sealed class AgentContext(
    Microsoft.SemanticKernel.Kernel kernel,
    OpenAIPromptExecutionSettings executionSettings,
    ILogger logger)
{
    /// <summary>
    /// Gets the Semantic Kernel instance associated with the agent.
    /// </summary>
    public Microsoft.SemanticKernel.Kernel Kernel { get; } = kernel ?? throw new ArgumentNullException(nameof(kernel));

    /// <summary>
    /// Gets the execution settings used for prompt invocation.
    /// </summary>
    public OpenAIPromptExecutionSettings ExecutionSettings { get; } = executionSettings ?? throw new ArgumentNullException(nameof(executionSettings));

    /// <summary>
    /// Gets the logger associated with the agent.
    /// </summary>
    public ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));
}
