---
name: scaffold-nexus-component
description: Guides the agent on how to scaffold a new frontend component or backend service for the Nexus Bakery & Tech platform following project conventions.
---

# Scaffolding Nexus Bakery & Tech Components

## Backend Service Scaffolding

When asked to create a new backend service:
1. Identify the domain (e.g., Auth, Products, Orders).
2. Create the Entity class in `src/NexusBakery.Domain/Entities/`. Ensure it uses `ObjectId` and has standard audit fields.
3. Create the generic repository interface in `src/NexusBakery.Domain/Interfaces/`.
4. Implement the repository using MongoDB C# driver in `src/NexusBakery.Infrastructure/Persistence/Repositories/`.
5. Create DTOs and Validation logic in `src/NexusBakery.Application/`.
6. Create the Service in `src/NexusBakery.Application/Services/`.
7. Wire up the REST Controller in `src/NexusBakery.Api/Controllers/`. Apply appropriate RBAC `[Authorize]` attributes.

## Frontend Component Scaffolding

When asked to create a frontend React component:
1. Place it in the appropriate `client/src/components/` subfolder.
2. Ensure you use standard React functional component syntax with Hooks.
3. Import icons from `lucide-react`.
4. State should be mapped to the relevant Zustand store (`client/src/store/`) or fetched via TanStack query (`client/src/hooks/`).
5. Use Vanilla CSS (BEM naming convention or CSS modules preferred) leveraging the design tokens from `index.css`. Adhere strictly to the Glassmorphic design pattern.
