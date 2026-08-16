---
name: fix-backend-error
description: Workflow for diagnosing and fixing C# compilation errors, runtime exceptions, or MongoDB issues in the ASP.NET Core backend.
---

# Fixing Backend Errors

When instructed to fix a backend error:

1. **Analyze the Stack Trace:**
   - Always read the exact exception message and line number first.
   - For `MongoWriteException` or `MongoBulkWriteException`, check if an index constraint (like unique email/SKU) was violated.
2. **Review Dependency Injection (DI):**
   - If you see `InvalidOperationException: Unable to resolve service`, immediately check `Program.cs` to ensure the repository, service, or tool was registered with the correct lifetime (`AddScoped`, `AddTransient`, `AddSingleton`).
3. **Verify SignalR Context:**
   - If errors occur during `AgentHub` broadcasting, ensure `IHubContext<AgentHub>` is being injected correctly and that you are awaiting the asynchronous `SendAsync` methods.
4. **Implement the Fix:**
   - Modify the necessary code and gracefully handle exceptions using `try/catch` where appropriate, especially during Payment Gateway HTTP calls or LLM API requests.
   - Always log errors using `ILogger<T>`.
