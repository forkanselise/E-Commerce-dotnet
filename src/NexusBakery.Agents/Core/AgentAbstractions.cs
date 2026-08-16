using System.Text.Json;

namespace NexusBakery.Agents.Core;

public class AgentMessage
{
    public string Role { get; set; } = "user"; // "user", "assistant", "system", "tool"
    public string Content { get; set; } = string.Empty;
    public string? ToolCallId { get; set; }
    public string? ToolName { get; set; }
}

public class AgentContext
{
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "anonymous";
    public string UserName { get; set; } = "Guest";
    public string UserRole { get; set; } = "User";
    public List<AgentMessage> History { get; set; } = new();

    // Streaming & Callback delegates
    public Func<string, string, Task>? OnThinking { get; set; }      // (agentName, thought)
    public Func<string, string, object, Task>? OnToolCall { get; set; }   // (agentName, toolName, input)
    public Func<string, string, object, long, Task>? OnToolResult { get; set; }   // (agentName, toolName, result, durationMs)
    public Func<string, string, string, Task>? OnDelegation { get; set; }  // (from, to, reason)
    public Func<string, string, bool, Task>? OnResponse { get; set; } // (agentName, text, isFinal)
    public Func<string, string, Task>? OnStatusUpdate { get; set; }   // (agentName, status)
    public Func<string, object, Task>? OnClientAction { get; set; }   // (actionName, payload)
}

public class ToolResult
{
    public bool Success { get; set; } = true;
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default);
}

public class AgentExecutionResult
{
    public string RespondingAgent { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> ToolsExecuted { get; set; } = new();
    public bool WasDelegated { get; set; }
    public string? DelegatedTo { get; set; }
}

public interface IAgent
{
    string Name { get; }
    string Description { get; }
    string SystemPrompt { get; }
    IReadOnlyList<IAgentTool> Tools { get; }

    Task<AgentExecutionResult> ExecuteAsync(
        AgentContext context,
        string userMessage,
        CancellationToken cancellationToken = default
    );
}
