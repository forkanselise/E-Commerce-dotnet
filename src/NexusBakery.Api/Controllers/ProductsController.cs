using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Services;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int limit = 8)
    {
        var result = await _productService.GetFeaturedProductsAsync(limit);
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _productService.GetCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var product = await _productService.GetProductBySlugAsync(slug);
        return product == null ? NotFound(new { message = $"Product with slug '{slug}' not found." }) : Ok(product);
    }

    [Authorize(Roles = "Admin,SystemAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var created = await _productService.CreateProductAsync(request, userId);
        return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
    }

    [Authorize(Roles = "Admin,SystemAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var updated = await _productService.UpdateProductAsync(id, request, userId);
        return updated == null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = "Admin,SystemAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var deleted = await _productService.DeleteProductAsync(id);
        return deleted ? Ok(new { message = "Product deleted successfully." }) : NotFound();
    }
}
