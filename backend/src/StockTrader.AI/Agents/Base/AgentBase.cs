using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
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

    /// <summary>
    /// Registers a plugin on the shared <see cref="Kernel"/> under the given name, unless a
    /// plugin with that name has already been registered.
    /// </summary>
    /// <remarks>
    /// <see cref="AgentContext"/> (and therefore its <see cref="Microsoft.SemanticKernel.Kernel"/>) is
    /// scoped per HTTP request and shared across every agent constructed within that scope
    /// (e.g. by <see cref="StockTrader.AI.Agents.Factory.AgentFactory"/>). Multiple agents can depend
    /// on the same plugin (e.g. both <c>MarketAgent</c> and <c>ResearchAgent</c> use "CompanyProfile"),
    /// so registration must be idempotent to avoid
    /// "An item with the same key has already been added" from <c>KernelPluginCollection.Add</c>.
    /// </remarks>
    protected void RegisterPluginIfMissing(string pluginName, object pluginInstance)
    {
        if (!Kernel.Plugins.TryGetPlugin(pluginName, out _))
        {
            Kernel.Plugins.AddFromObject(pluginInstance, pluginName);
        }
    }
}
