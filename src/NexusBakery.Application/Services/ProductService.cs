using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;
using NexusBakery.Domain.Interfaces;
using NexusBakery.Domain.Entities;

namespace NexusBakery.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<object> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<string> CreateAsync(CreateProductDto dto) 
    {
        var product = new Product
        {
            Title = dto.Title,
            Slug = dto.Slug ?? dto.Title?.ToLower().Replace(" ", "-"),
            Sku = dto.Sku,
            Category = (NexusBakery.Domain.Enums.ProductCategory)Enum.Parse(typeof(NexusBakery.Domain.Enums.ProductCategory), dto.Category ?? "Ingredients", true),
            Price = dto.Price,
            WarehouseStock = dto.WarehouseStock,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            IsAvailable = dto.IsAvailable
        };
        var created = await _productRepository.CreateAsync(product);
        return created.Id;
    }

    public async Task<Product?> UpdateAsync(string id, UpdateProductDto dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title)) existing.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Sku)) existing.Sku = dto.Sku;
        if (!string.IsNullOrWhiteSpace(dto.ShortDescription)) existing.ShortDescription = dto.ShortDescription;
        if (!string.IsNullOrWhiteSpace(dto.Description)) existing.Description = dto.Description;
        if (dto.Price > 0) existing.Price = dto.Price;
        if (dto.WarehouseStock >= 0) existing.WarehouseStock = dto.WarehouseStock;

        if (!string.IsNullOrWhiteSpace(dto.Category))
        {
            if (Enum.TryParse<NexusBakery.Domain.Enums.ProductCategory>(dto.Category, true, out var catEnum))
            {
                existing.Category = catEnum;
            }
        }

        await _productRepository.UpdateAsync(existing);
        return existing;
    }

    public Task AddImageAsync(string id, string url) => Task.CompletedTask;
}
