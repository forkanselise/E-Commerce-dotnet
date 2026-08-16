using NexusBakery.Agents.Agents;
using NexusBakery.Agents.Core;

namespace NexusBakery.Agents.Core;

public class AgentOrchestrator
{
    private readonly RouterConciergeAgent _routerAgent;

    public AgentOrchestrator(RouterConciergeAgent routerAgent)
    {
        _routerAgent = routerAgent;
    }

    public async Task<AgentExecutionResult> ProcessMessageAsync(
        AgentContext context,
        string message,
        CancellationToken cancellationToken = default)
    {
        context.History.Add(new AgentMessage { Role = "user", Content = message });

        var result = await _routerAgent.ExecuteAsync(context, message, cancellationToken);

        context.History.Add(new AgentMessage { Role = "assistant", Content = result.Message });

        if (context.OnResponse != null)
        {
            await context.OnResponse(result.RespondingAgent, result.Message, true);
        }

        return result;
    }
}
