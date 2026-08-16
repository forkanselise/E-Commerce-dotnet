---
name: create-agent-tool
description: Standard operating procedure for adding a new tool to the Multi-Agent AI Engine (Router, Storefront, Masterclass, or Warehouse).
---

# Creating a New Agent Tool

When instructed to add a new capability/tool to the AI Engine:

1. **Create the Tool Class:**
   - Location: `src/NexusBakery.Agents/Tools/`
   - Implement the `IAgentTool` interface.
   - Define the tool `Name` and `Description` clearly so the LLM knows when to pick it.
   - Define the `ParameterSchema` using a strictly typed JSON Schema structure.

2. **Implement InvokeAsync:**
   - Inside `InvokeAsync()`, extract parameters from the `JsonElement`.
   - Check RBAC permissions using `AgentContext.UserRole` if the tool mutates data (especially for `WarehouseOpsAgent` tools).
   - Return a `ToolResult` containing the serialized JSON of the operation outcome.

3. **Register the Tool:**
   - Location: `src/NexusBakery.Agents/Core/ToolRegistry.cs`
   - Add the new tool to the appropriate agent's list in the dependency injection container.

4. **Update the System Prompt:**
   - Location: `src/NexusBakery.Agents/Prompts/`
   - Update the respective agent's `.txt` prompt file to briefly mention the existence and proper use-case of the new tool.
