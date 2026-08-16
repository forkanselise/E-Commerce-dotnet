---
name: scaffold-signalr-hub
description: Guidelines for creating or modifying a real-time SignalR WebSocket hub and connecting it to the Vite+React frontend.
---

# Scaffolding a SignalR Hub

When asked to implement a new real-time feature:

## Backend Hub Creation
1. **Create the Hub Class:**
   - Location: `src/NexusBakery.Api/Hubs/`
   - Inherit from `Hub` or `Hub<ITypedClient>`.
   - Override `OnConnectedAsync()` and `OnDisconnectedAsync()` to manage connection groups (e.g., joining `user_{UserId}` groups).
2. **Register the Hub:**
   - Location: `src/NexusBakery.Api/Program.cs`
   - Map the hub endpoint: `app.MapHub<YourHub>("/hubs/your-hub-path");`
3. **Trigger Events from Services:**
   - Inject `IHubContext<YourHub>` into Application layer services to broadcast updates when database states change (e.g., inventory updates, active visitor counts).

## Frontend Connection
1. **Create the Connection Store:**
   - Location: `client/src/store/` or inside a dedicated `SignalRProvider.jsx`.
   - Use `@microsoft/signalr` to build a `HubConnection`.
   - Implement an exponential backoff retry policy for `withAutomaticReconnect()`.
2. **Listen to Events:**
   - Wire `connection.on("EventName", (data) => { ... })` into Zustand stores so the UI automatically reacts to incoming socket data.
