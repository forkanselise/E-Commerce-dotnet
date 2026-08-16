using System.Text.Json;
using NexusBakery.Agents.Core;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Agents.Tools;

public class SearchProductsTool : IAgentTool
{
    private readonly IProductRepository _productRepository;

    public SearchProductsTool(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public string Name => "SearchProducts";
    public string Description => "Searches the Nexus Bakery catalog for physical products, tools, and kitchen electronics by keyword or category.";

    public async Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default)
    {
        string? query = parameters.TryGetProperty("query", out var q) ? q.GetString() : null;
        string? categoryStr = parameters.TryGetProperty("category", out var c) ? c.GetString() : null;

        ProductCategory? category = null;
        if (!string.IsNullOrEmpty(categoryStr) && Enum.TryParse<ProductCategory>(categoryStr, true, out var parsedCat))
        {
            category = parsedCat;
        }

        var (items, total) = await _productRepository.SearchProductsAsync(query, category, null, null, null, true, null, 1, 5, cancellationToken);

        var simplified = items.Select(p => new
        {
            id = p.Id,
            title = p.Title,
            slug = p.Slug,
            price = p.Price,
            category = p.Category.ToString(),
            stock = p.WarehouseStock,
            shortDescription = p.ShortDescription,
            url = $"/store/{p.Slug}"
        });

        return new ToolResult { Success = true, Data = new { totalFound = total, products = simplified } };
    }
}

public class CheckStockTool : IAgentTool
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public CheckStockTool(IProductRepository productRepository, IWarehouseRepository warehouseRepository)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public string Name => "CheckStock";
    public string Description => "Checks live inventory stock for a product by title or slug.";

    public async Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default)
    {
        string? query = parameters.TryGetProperty("query", out var q) ? q.GetString() : null;
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolResult { Success = false, ErrorMessage = "A product title or slug is required to check stock." };
        }

        var (items, _) = await _productRepository.SearchProductsAsync(query, null, null, null, null, null, null, 1, 1, cancellationToken);
        var product = items.FirstOrDefault();

        if (product == null)
        {
            return new ToolResult { Success = false, ErrorMessage = $"No product found matching '{query}'." };
        }

        var inventory = await _warehouseRepository.GetByProductIdAsync(product.Id, cancellationToken);

        return new ToolResult
        {
            Success = true,
            Data = new
            {
                productId = product.Id,
                title = product.Title,
                sku = product.Sku,
                availableStock = inventory?.AvailableStock ?? product.WarehouseStock,
                isAvailable = product.IsAvailable && (inventory?.AvailableStock ?? product.WarehouseStock) > 0,
                price = product.Price
            }
        };
    }
}

public class AddToCartTool : IAgentTool
{
    private readonly IProductRepository _productRepository;

    public AddToCartTool(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public string Name => "AddToCart";
    public string Description => "Adds a product directly to the customer's shopping cart. Emits a real-time event to the frontend UI.";

    public async Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default)
    {
        string? query = parameters.TryGetProperty("product", out var p) ? p.GetString() : null;
        int quantity = parameters.TryGetProperty("quantity", out var q) ? q.GetInt32() : 1;

        if (string.IsNullOrWhiteSpace(query))
        {
            return new ToolResult { Success = false, ErrorMessage = "Please specify which product you would like to add to cart." };
        }

        var (items, _) = await _productRepository.SearchProductsAsync(query, null, null, null, null, null, null, 1, 1, cancellationToken);
        var product = items.FirstOrDefault();

        if (product == null)
        {
            return new ToolResult { Success = false, ErrorMessage = $"Could not find product '{query}' in our catalog." };
        }

        if (product.WarehouseStock < quantity)
        {
            return new ToolResult { Success = false, ErrorMessage = $"Sorry, '{product.Title}' only has {product.WarehouseStock} units in stock." };
        }

        var cartPayload = new
        {
            productId = product.Id,
            title = product.Title,
            slug = product.Slug,
            price = product.Price,
            quantity = quantity,
            thumbnail = product.Images.FirstOrDefault()?.Url,
            sku = product.Sku
        };

        if (context.OnClientAction != null)
        {
            await context.OnClientAction("AddToCart", cartPayload);
        }

        return new ToolResult
        {
            Success = true,
            Data = new
            {
                message = $"Successfully added {quantity}x '{product.Title}' to cart.",
                product = cartPayload
            }
        };
    }
}

public class SearchTutorialsTool : IAgentTool
{
    private readonly ITutorialRepository _tutorialRepository;

    public SearchTutorialsTool(ITutorialRepository tutorialRepository)
    {
        _tutorialRepository = tutorialRepository;
    }

    public string Name => "SearchTutorials";
    public string Description => "Searches the Masterclass video tutorials, chapters, recipes, and instructors.";

    public async Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default)
    {
        string? query = parameters.TryGetProperty("query", out var q) ? q.GetString() : null;
        var (items, total) = await _tutorialRepository.SearchTutorialsAsync(query, null, null, 1, 3, cancellationToken);

        var simplified = items.Select(t => new
        {
            id = t.Id,
            title = t.Title,
            slug = t.Slug,
            skillLevel = t.SkillLevel.ToString(),
            duration = $"{t.DurationMinutes} mins",
            instructor = t.Instructor.Name,
            chapters = t.Chapters.Select(c => $"{c.TimestampDisplay} - {c.Title}").ToList(),
            ingredients = t.Ingredients,
            url = $"/masterclass/{t.Slug}"
        });

        return new ToolResult { Success = true, Data = new { totalFound = total, tutorials = simplified } };
    }
}

public class AdjustInventoryTool : IAgentTool
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;

    public AdjustInventoryTool(IWarehouseRepository warehouseRepository, IProductRepository productRepository)
    {
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
    }

    public string Name => "AdjustInventory";
    public string Description => "Admin-only tool to modify warehouse stock quantities using natural language.";

    public async Task<ToolResult> InvokeAsync(JsonElement parameters, AgentContext context, CancellationToken cancellationToken = default)
    {
        if (context.UserRole is not ("Admin" or "SystemAdmin"))
        {
            return new ToolResult { Success = false, ErrorMessage = "Unauthorized: Only Admins can modify warehouse inventory." };
        }

        string? query = parameters.TryGetProperty("product", out var p) ? p.GetString() : null;
        if (!parameters.TryGetProperty("newStock", out var s))
        {
            return new ToolResult { Success = false, ErrorMessage = "New stock quantity must be specified." };
        }
        int newStock = s.GetInt32();
        string reason = parameters.TryGetProperty("reason", out var r) ? r.GetString() ?? "AI Agent Adjustment" : "AI Agent Adjustment";

        var (items, _) = await _productRepository.SearchProductsAsync(query, null, null, null, null, null, null, 1, 1, cancellationToken);
        var product = items.FirstOrDefault();
        if (product == null)
        {
            return new ToolResult { Success = false, ErrorMessage = $"Product '{query}' not found." };
        }

        var success = await _warehouseRepository.AdjustStockAsync(
            product.Id,
            newStock,
            context.UserId,
            context.UserName,
            context.UserRole,
            reason,
            cancellationToken);

        if (success)
        {
            product.WarehouseStock = newStock;
            product.IsAvailable = newStock > 0;
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        return new ToolResult
        {
            Success = success,
            Data = new
            {
                productId = product.Id,
                title = product.Title,
                previousStock = product.WarehouseStock,
                newStock = newStock,
                updatedBy = context.UserName
            }
        };
    }
}
