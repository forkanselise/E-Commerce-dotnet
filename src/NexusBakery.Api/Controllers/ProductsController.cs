using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileStorageService;

    public ProductsController(IProductService productService, IFileStorageService fileStorageService)
    {
        _productService = productService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin, SystemAdmin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var id = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetProductById), new { id }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(string id) => Ok();

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
