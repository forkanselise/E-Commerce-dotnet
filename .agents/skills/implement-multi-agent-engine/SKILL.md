---
name: implement-multi-agent-engine
description: Runbook for building the core components of the Nexus Bakery Multi-Agent AI Engine (Orchestrator, Router, and Specialist Agents).
---

# Implementing the Multi-Agent Engine

When instructed to build or modify the Multi-Agent Engine backend:

1. **Agent Orchestrator (`src/NexusBakery.Agents/Core/AgentOrchestrator.cs`)**
   - The Orchestrator is the entry point. It instantiates the `AgentContext` and wires up the SignalR `AgentHub` delegates for real-time streaming (`OnThinking`, `OnToolCall`, `OnResponse`).
   - It always invokes the `RouterConciergeAgent` first.

2. **Router Concierge (`RouterConciergeAgent.cs`)**
   - The Router does NOT answer domain questions directly.
   - Its primary job is **Intent Classification** & **Domain Boundary Enforcement**:
     * If the query is outside the Nexus Bakery & Tech domain (e.g. general knowledge, external topics), refuse immediately with: *"I can only answer questions related to Nexus Bakery & Tech products, tools, baking tutorials, and your shopping cart. I can't assist with queries outside my domain."*
     * If in-domain, invoke `DelegateToAgent(agentName, reason)` to route to `StorefrontInventory`, `BakingMasterclass`, or `WarehouseOps`.

3. **Specialist Agents (`StorefrontInventory`, `BakingMasterclass`, `WarehouseOps`)**
   - These agents receive the delegated context.
   - They must utilize the tools registered in `ToolRegistry.cs` to fetch live MongoDB data.
   - `StorefrontInventory` includes the **`AddToCart`** tool, which triggers `context.OnClientAction("AddToCart", item)` to update the client's shopping cart in real time.
   - For `WarehouseOps`, strictly verify `context.UserRole` before allowing any tool that mutates stock.

4. **Streaming Protocol**
   - Every step an agent takes (thinking, calling a tool, getting a result, delegating, client action) MUST trigger the appropriate `context.On...` callback so the React frontend AI Drawer can animate the execution log for the user.
