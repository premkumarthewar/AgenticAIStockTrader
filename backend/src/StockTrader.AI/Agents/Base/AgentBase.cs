using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace StockTrader.AI.Agents.Base;

/// <summary>
/// Base class for all AI agents.
/// </summary>
public abstract class AgentBase(AgentContext context)
{
    /// <summary>
    /// Gets the runtime context for this agent.
    /// </summary>
    protected AgentContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets the Semantic Kernel instance.
    /// </summary>
    protected Microsoft.SemanticKernel.Kernel Kernel => Context.Kernel;

    /// <summary>
    /// Gets the execution settings.
    /// </summary>
    protected OpenAIPromptExecutionSettings ExecutionSettings
        => Context.ExecutionSettings;

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger Logger
        => Context.Logger;
}
