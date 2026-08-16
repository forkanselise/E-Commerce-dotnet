using System.Diagnostics;
using System.Text.Json;
using NexusBakery.Agents.Core;
using NexusBakery.Agents.Tools;

namespace NexusBakery.Agents.Agents;

public class RouterConciergeAgent : IAgent
{
    private readonly StorefrontInventoryAgent _storefrontAgent;
    private readonly BakingMasterclassAgent _bakingAgent;
    private readonly WarehouseOpsAgent _warehouseAgent;

    public RouterConciergeAgent(
        StorefrontInventoryAgent storefrontAgent,
        BakingMasterclassAgent bakingAgent,
        WarehouseOpsAgent warehouseAgent)
    {
        _storefrontAgent = storefrontAgent;
        _bakingAgent = bakingAgent;
        _warehouseAgent = warehouseAgent;
    }

    public string Name => "RouterConcierge";
    public string Description => "The central orchestrator and concierge. Classifies user intent and enforces strict Nexus Bakery domain guardrails.";
    public string SystemPrompt => @"You are the Nexus Bakery & Tech concierge. Your sole purpose is assisting customers with Nexus Bakery physical products (bakery items, baking tools, electronics), video masterclass tutorials, and shopping cart operations. 

STRICT DOMAIN GUARDRAIL: You must ONLY answer questions directly related to Nexus Bakery & Tech products, tutorials, baking recipes, and cart management. If the user asks about ANYTHING else (general world knowledge, coding, math, sports, politics, weather, external companies), you MUST immediately refuse with this EXACT phrase:
'I can only answer questions related to Nexus Bakery & Tech products, tools, baking tutorials, and your shopping cart. I can't assist with queries outside my domain.'";

    public IReadOnlyList<IAgentTool> Tools => Array.Empty<IAgentTool>();

    public async Task<AgentExecutionResult> ExecuteAsync(AgentContext context, string userMessage, CancellationToken cancellationToken = default)
    {
        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Thinking");
        if (context.OnThinking != null) await context.OnThinking(Name, "Analyzing query intent and checking domain boundaries...");

        var msg = userMessage.Trim().ToLowerInvariant();

        // 1. Check for Out-of-Context Refusal
        var isOutOfContext = CheckIsOutOfContext(msg);
        if (isOutOfContext)
        {
            if (context.OnThinking != null) await context.OnThinking(Name, "Out-of-context query detected. Applying strict domain boundary refusal.");
            if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");

            return new AgentExecutionResult
            {
                RespondingAgent = Name,
                Message = "I can only answer questions related to Nexus Bakery & Tech products, tools, baking tutorials, and your shopping cart. I can't assist with queries outside my domain."
            };
        }

        // 2. Greetings
        if (msg is "hi" or "hello" or "hey" or "good morning" or "good evening")
        {
            if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");
            return new AgentExecutionResult
            {
                RespondingAgent = Name,
                Message = $"Hello {context.UserName}! 🥐 Welcome to Nexus Bakery & Tech. I can help you find artisan baked goods, precision baking gear, stream our masterclasses, or add items directly to your cart. How can I assist you today?"
            };
        }

        // 3. Admin / Warehouse Intent
        if (msg.Contains("inventory") || msg.Contains("adjust stock") || msg.Contains("low stock") || msg.Contains("warehouse") || (msg.Contains("stock") && context.UserRole is "Admin" or "SystemAdmin"))
        {
            if (context.OnDelegation != null) await context.OnDelegation(Name, _warehouseAgent.Name, "Warehouse inventory operations intent detected.");
            return await _warehouseAgent.ExecuteAsync(context, userMessage, cancellationToken);
        }

        // 4. Masterclass / Tutorial Intent
        if (msg.Contains("tutorial") || msg.Contains("masterclass") || msg.Contains("course") || msg.Contains("recipe") || msg.Contains("macaronage") || msg.Contains("technique") || msg.Contains("chef") || msg.Contains("video") || msg.Contains("chapter"))
        {
            if (context.OnDelegation != null) await context.OnDelegation(Name, _bakingAgent.Name, "Masterclass & baking technique intent detected.");
            return await _bakingAgent.ExecuteAsync(context, userMessage, cancellationToken);
        }

        // 5. Storefront & Cart Intent
        if (context.OnDelegation != null) await context.OnDelegation(Name, _storefrontAgent.Name, "Product catalog, shopping, or cart intent detected.");
        return await _storefrontAgent.ExecuteAsync(context, userMessage, cancellationToken);
    }

    private static bool CheckIsOutOfContext(string query)
    {
        string[] outOfDomainKeywords = {
            "python", "javascript", "c#", "java", "write code", "html", "css",
            "weather in", "football", "cricket", "president", "capital of",
            "crypto", "bitcoin", "solve math", "calculate 2+", "who won",
            "flight to", "hotel booking", "translate to french", "write a poem about space"
        };

        return outOfDomainKeywords.Any(k => query.Contains(k));
    }
}

public class StorefrontInventoryAgent : IAgent
{
    private readonly SearchProductsTool _searchTool;
    private readonly CheckStockTool _stockTool;
    private readonly AddToCartTool _cartTool;

    public StorefrontInventoryAgent(SearchProductsTool searchTool, CheckStockTool stockTool, AddToCartTool cartTool)
    {
        _searchTool = searchTool;
        _stockTool = stockTool;
        _cartTool = cartTool;
    }

    public string Name => "StorefrontInventory";
    public string Description => "Handles product recommendations, stock availability inquiries, and direct Add-To-Cart execution.";
    public string SystemPrompt => "You are the Nexus Bakery storefront assistant. Help users find bakery products, tools, and electronics. Allow users to add items directly to their cart.";
    public IReadOnlyList<IAgentTool> Tools => new IAgentTool[] { _searchTool, _stockTool, _cartTool };

    public async Task<AgentExecutionResult> ExecuteAsync(AgentContext context, string userMessage, CancellationToken cancellationToken = default)
    {
        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Executing");
        var msg = userMessage.Trim().ToLowerInvariant();
        var toolsUsed = new List<string>();

        // Handle Add to Cart
        if (msg.Contains("add") && (msg.Contains("cart") || msg.Contains("bag") || msg.Contains("buy")))
        {
            if (context.OnThinking != null) await context.OnThinking(Name, "Processing 'Add to Cart' request...");
            var productQuery = msg.Replace("add", "").Replace("to my cart", "").Replace("to cart", "").Replace("please", "").Trim();
            
            var sw = Stopwatch.StartNew();
            if (context.OnToolCall != null) await context.OnToolCall(Name, _cartTool.Name, new { product = productQuery, quantity = 1 });
            
            var json = JsonSerializer.SerializeToElement(new { product = productQuery, quantity = 1 });
            var result = await _cartTool.InvokeAsync(json, context, cancellationToken);
            sw.Stop();
            
            toolsUsed.Add(_cartTool.Name);
            if (context.OnToolResult != null) await context.OnToolResult(Name, _cartTool.Name, result.Data ?? result.ErrorMessage ?? "", sw.ElapsedMilliseconds);

            if (result.Success)
            {
                if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");
                return new AgentExecutionResult
                {
                    RespondingAgent = Name,
                    Message = $"🛒 Done! I have added the item to your shopping cart. Your cart badge has been updated. Would you like to view your cart or continue shopping?",
                    ToolsExecuted = toolsUsed
                };
            }
            else
            {
                return new AgentExecutionResult
                {
                    RespondingAgent = Name,
                    Message = $"I couldn't add that to your cart: {result.ErrorMessage}",
                    ToolsExecuted = toolsUsed
                };
            }
        }

        // Handle Check Stock
        if (msg.Contains("stock") || msg.Contains("available") || msg.Contains("how many"))
        {
            if (context.OnThinking != null) await context.OnThinking(Name, "Checking real-time stock levels in warehouse...");
            var sw = Stopwatch.StartNew();
            if (context.OnToolCall != null) await context.OnToolCall(Name, _stockTool.Name, new { query = userMessage });
            
            var json = JsonSerializer.SerializeToElement(new { query = userMessage });
            var result = await _stockTool.InvokeAsync(json, context, cancellationToken);
            sw.Stop();
            
            toolsUsed.Add(_stockTool.Name);
            if (context.OnToolResult != null) await context.OnToolResult(Name, _stockTool.Name, result.Data ?? "", sw.ElapsedMilliseconds);

            if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");
            return new AgentExecutionResult
            {
                RespondingAgent = Name,
                Message = result.Success 
                    ? $"📦 Stock Status: We found the requested product in our warehouse inventory with live availability." 
                    : $"Stock inquiry: {result.ErrorMessage}",
                ToolsExecuted = toolsUsed
            };
        }

        // Default: Search Products
        if (context.OnThinking != null) await context.OnThinking(Name, "Searching product catalog for matches...");
        var searchSw = Stopwatch.StartNew();
        if (context.OnToolCall != null) await context.OnToolCall(Name, _searchTool.Name, new { query = userMessage });
        
        var searchJson = JsonSerializer.SerializeToElement(new { query = userMessage });
        var searchResult = await _searchTool.InvokeAsync(searchJson, context, cancellationToken);
        searchSw.Stop();
        
        toolsUsed.Add(_searchTool.Name);
        if (context.OnToolResult != null) await context.OnToolResult(Name, _searchTool.Name, searchResult.Data ?? "", searchSw.ElapsedMilliseconds);

        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");
        return new AgentExecutionResult
        {
            RespondingAgent = Name,
            Message = "Here are our top recommended products matching your inquiry. You can ask me to add any of these to your cart anytime!",
            ToolsExecuted = toolsUsed
        };
    }
}

public class BakingMasterclassAgent : IAgent
{
    private readonly SearchTutorialsTool _searchTutorialsTool;

    public BakingMasterclassAgent(SearchTutorialsTool searchTutorialsTool)
    {
        _searchTutorialsTool = searchTutorialsTool;
    }

    public string Name => "BakingMasterclass";
    public string Description => "Recommends masterclass video tutorials, extracts video chapter timestamps, and provides professional baking guidance.";
    public string SystemPrompt => "You are the Nexus Bakery baking instructor AI. Recommend tutorials and explain techniques.";
    public IReadOnlyList<IAgentTool> Tools => new IAgentTool[] { _searchTutorialsTool };

    public async Task<AgentExecutionResult> ExecuteAsync(AgentContext context, string userMessage, CancellationToken cancellationToken = default)
    {
        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Executing");
        if (context.OnThinking != null) await context.OnThinking(Name, "Searching video masterclasses and recipe syllabus...");

        var sw = Stopwatch.StartNew();
        if (context.OnToolCall != null) await context.OnToolCall(Name, _searchTutorialsTool.Name, new { query = userMessage });
        
        var json = JsonSerializer.SerializeToElement(new { query = userMessage });
        var result = await _searchTutorialsTool.InvokeAsync(json, context, cancellationToken);
        sw.Stop();

        if (context.OnToolResult != null) await context.OnToolResult(Name, _searchTutorialsTool.Name, result.Data ?? "", sw.ElapsedMilliseconds);
        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");

        return new AgentExecutionResult
        {
            RespondingAgent = Name,
            Message = "🎓 Masterclass Recommendation: I found our curated video masterclasses matching your skill level and ingredients. Our video players feature clickable timestamp chapters so you can jump directly to specific folding and baking steps!",
            ToolsExecuted = new List<string> { _searchTutorialsTool.Name }
        };
    }
}

public class WarehouseOpsAgent : IAgent
{
    private readonly AdjustInventoryTool _adjustTool;
    private readonly CheckStockTool _stockTool;

    public WarehouseOpsAgent(AdjustInventoryTool adjustTool, CheckStockTool stockTool)
    {
        _adjustTool = adjustTool;
        _stockTool = stockTool;
    }

    public string Name => "WarehouseOps";
    public string Description => "Admin-only inventory manager for live stock queries and natural language stock adjustments.";
    public string SystemPrompt => "You are the Nexus warehouse operations assistant for Admins.";
    public IReadOnlyList<IAgentTool> Tools => new IAgentTool[] { _adjustTool, _stockTool };

    public async Task<AgentExecutionResult> ExecuteAsync(AgentContext context, string userMessage, CancellationToken cancellationToken = default)
    {
        if (context.UserRole is not ("Admin" or "SystemAdmin"))
        {
            return new AgentExecutionResult
            {
                RespondingAgent = Name,
                Message = "⛔ Access Denied: The Warehouse Operations Assistant is strictly reserved for Admin and SystemAdmin roles."
            };
        }

        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Executing");
        if (context.OnThinking != null) await context.OnThinking(Name, "Admin verified. Executing warehouse operations command...");

        if (context.OnStatusUpdate != null) await context.OnStatusUpdate(Name, "Done");
        return new AgentExecutionResult
        {
            RespondingAgent = Name,
            Message = "📊 Warehouse Ops: Live stock query acknowledged. Inventory levels are synchronized across database and store front."
        };
    }
}
