using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService, 
        IFileStorageService fileStorageService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin, SystemAdmin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Product payload cannot be null." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Product title is required." });
        }

        try
        {
            var product = await _productService.CreateAsync(dto);
            return StatusCode(StatusCodes.Status201Created, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating product: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "Duplicate key constraint error when creating product");
            return Conflict(new { message = "A product with this SKU or slug already exists." });
        }
        catch (MongoBulkWriteException ex) when (System.Linq.Enumerable.Any(ex.WriteErrors, e => e.Category == ServerErrorCategory.DuplicateKey))
        {
            _logger.LogWarning(ex, "Duplicate key bulk write error when creating product");
            return Conflict(new { message = "A product with this SKU or slug already exists." });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Database timeout when creating product");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database connection timed out. Please verify MongoDB connectivity.", error = ex.Message });
        }
        catch (MongoConnectionException ex)
        {
            _logger.LogError(ex, "Database connection failed when creating product");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Database connection failed. Please verify MongoDB connectivity.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error when creating product");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create product", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound(new { message = "Product not found" });
        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, SystemAdmin")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto dto)
    {
        var updatedProduct = await _productService.UpdateAsync(id, dto);
        if (updatedProduct == null) return NotFound(new { message = "Product not found" });
        return Ok(updatedProduct);
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "Admin, SystemAdmin")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadImage(string id, IFormFile file)
    {
        var url = await _fileStorageService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType, "products");
        await _productService.AddImageAsync(id, url);
        return Ok(new { Url = url });
    }
}
