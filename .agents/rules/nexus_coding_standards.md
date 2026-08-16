---
name: Nexus Bakery & Tech Rules
description: General coding standards, architecture constraints, and styling guidelines for the Nexus Bakery & Tech platform.
trigger: always_on
---

# Nexus Bakery & Tech - Coding Standards & Rules

## 1. Backend Architecture (ASP.NET Core 8/9)
- **Clean Architecture:** Strictly follow the API, Application, Domain, and Infrastructure layer separation.
- **Data Access:** Use MongoDB C# Driver. Do NOT use Entity Framework Core unless explicitly requested. Always utilize `BsonId` and `BsonRepresentation(BsonType.ObjectId)` for IDs.
- **Dependency Injection:** Register all services and repositories via `IServiceCollection` in `Program.cs`.
- **Real-Time:** Use `SignalR` for any live updates (e.g., VisitorHub, AgentHub).
- **Authentication:** Use JWT Bearer tokens. Controller endpoints must be protected by `[Authorize]` with appropriate roles (`User`, `Admin`, `SystemAdmin`).

## 2. Frontend Architecture (Vite + React)
- **State Management:** Use `Zustand` for global client state and `TanStack React Query` for server state and caching.
- **Styling:** Use Vanilla CSS with CSS Variables for design tokens. Follow the "Glassmorphic Artisan + Tech" design aesthetic. Avoid Tailwind unless specified.
- **Components:** Keep components small, reusable, and functionally pure. Use Lucide-React for iconography.
- **API Communication:** Centralize Axios/fetch logic in a `services/` directory.

## 3. Database Guidelines (MongoDB)
- Always include `createdAt` and `updatedAt` timestamps.
- Use atomic operators (`$inc`, `$set`, `$push`) for updates, especially for inventory management.
- Denormalize data where it makes sense for read performance (e.g., embedding small arrays like video chapters, referencing large documents like user details).

## 4. Multi-Agent Engine
- All agent interactions must stream via the `AgentHub` SignalR connection.
- Implement strict RBAC checks before executing any modifying tool (e.g., adjusting warehouse inventory).
