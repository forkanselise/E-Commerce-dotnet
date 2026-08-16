using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NexusBakery.Agents.Core;
using NexusBakery.Api.Hubs;

namespace NexusBakery.Api.Controllers;

public class ChatMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AgentOrchestrator _orchestrator;
    private readonly IHubContext<AgentHub> _agentHub;

    public AgentController(AgentOrchestrator orchestrator, IHubContext<AgentHub> agentHub)
    {
        _orchestrator = orchestrator;
        _agentHub = agentHub;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message content cannot be empty." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Guest";
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();

        var context = new AgentContext
        {
            ConversationId = conversationId,
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            OnThinking = async (agent, thought) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("AgentThinking", new { conversationId, agentName = agent, thought, timestamp = DateTime.UtcNow });
            },
            OnToolCall = async (agent, tool, input) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("AgentToolCall", new { conversationId, agentName = agent, toolName = tool, toolInput = input, timestamp = DateTime.UtcNow });
            },
            OnToolResult = async (agent, tool, result, duration) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("AgentToolResult", new { conversationId, agentName = agent, toolName = tool, result, durationMs = duration, timestamp = DateTime.UtcNow });
            },
            OnDelegation = async (from, to, reason) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("AgentDelegation", new { conversationId, fromAgent = from, toAgent = to, reason, timestamp = DateTime.UtcNow });
            },
            OnStatusUpdate = async (agent, status) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("AgentStatusUpdate", new { conversationId, agentName = agent, status, timestamp = DateTime.UtcNow });
            },
            OnClientAction = async (action, payload) =>
            {
                await _agentHub.Clients.Group($"user_{userId}")
                    .SendAsync("ClientAction", new { conversationId, action, payload, timestamp = DateTime.UtcNow });
            }
        };

        var result = await _orchestrator.ProcessMessageAsync(context, request.Message);

        return Ok(new
        {
            conversationId,
            respondingAgent = result.RespondingAgent,
            message = result.Message,
            toolsExecuted = result.ToolsExecuted
        });
    }
}
