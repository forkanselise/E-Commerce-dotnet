using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.DTOs.Warehouse;
using NexusBakery.Application.Services;

namespace NexusBakery.Api.Controllers;

[Authorize(Roles = "Admin,SystemAdmin")]
[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetAllInventory()
    {
        var items = await _warehouseService.GetAllInventoryAsync();
        return Ok(items);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await _warehouseService.GetLowStockItemsAsync();
        return Ok(items);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProductId(string productId)
    {
        var item = await _warehouseService.GetByProductIdAsync(productId);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("adjust-stock/{productId}")]
    public async Task<IActionResult> AdjustStock(string productId, [FromBody] AdjustStockRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Store Admin";
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Admin";

        var success = await _warehouseService.AdjustStockAsync(
            productId,
            request.NewStock,
            userId,
            userName,
            userRole,
            request.Reason);

        return success 
            ? Ok(new { message = $"Stock adjusted to {request.NewStock} successfully." }) 
            : NotFound(new { message = $"Inventory record for product '{productId}' not found." });
    }
}
