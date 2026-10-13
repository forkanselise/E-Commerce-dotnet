using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Application.Interfaces;
using NexusBakery.Domain.Interfaces;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using System.Linq;

namespace NexusBakery.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository? _warehouseRepository;

    public ProductService(IProductRepository productRepository, IWarehouseRepository? warehouseRepository = null)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<object> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public static ProductCategory ParseProductCategory(string? categoryStr)
    {
        if (string.IsNullOrWhiteSpace(categoryStr))
        {
            return ProductCategory.Bakery;
        }

        var normalized = categoryStr.Trim().Replace(" ", "").Replace("&", "").Replace("_", "").Replace("-", "").ToLowerInvariant();

        return normalized switch
        {
            "bakery" or "bakes" or "bakerycoffee" or "coffee" or "cakes" or "donuts" or "cookies" or "brownies" or "drinks" => ProductCategory.Bakery,
            "bakingtools" or "tools" or "bakingtool" or "equipment" => ProductCategory.BakingTools,
            "electronics" or "electronic" or "tech" => ProductCategory.Electronics,
            "ingredients" or "ingredient" => ProductCategory.Ingredients,
            "packaging" or "boxes" => ProductCategory.Packaging,
            "decorations" or "decoratingtools" => ProductCategory.Decorations,
            "moulds" or "molds" => ProductCategory.Moulds,
            _ => Enum.TryParse<ProductCategory>(categoryStr, true, out var parsed) ? parsed : ProductCategory.Bakery
        };
    }

    public async Task<Product> CreateAsync(CreateProductDto dto) 
    {
        ArgumentNullException.ThrowIfNull(dto);

        var category = ParseProductCategory(dto.Category);
        var title = string.IsNullOrWhiteSpace(dto.Title) ? "New Product" : dto.Title.Trim();

        var baseSlug = !string.IsNullOrWhiteSpace(dto.Slug)
            ? dto.Slug.Trim().ToLowerInvariant()
            : Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "product-" + Guid.NewGuid().ToString("N")[..6];
        }

        var slug = baseSlug;
        var existingWithSlug = await _productRepository.GetBySlugAsync(slug);
        if (existingWithSlug != null)
        {
            slug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var sku = !string.IsNullOrWhiteSpace(dto.Sku)
            ? dto.Sku.Trim()
            : $"SKU-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var existingWithSku = await _productRepository.GetBySkuAsync(sku);
        if (existingWithSku != null)
        {
            sku = $"{sku}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
        }

        var images = dto.Images ?? new List<ProductImage>();
        if (images.Count == 0 && !string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            images.Add(new ProductImage
            {
                Url = dto.ImageUrl.Trim(),
                Alt = title,
                IsPrimary = true
            });
        }

        var product = new Product
        {
            Title = title,
            Slug = slug,
            Sku = sku,
            Category = category,
            SubCategory = !string.IsNullOrWhiteSpace(dto.SubCategory) ? dto.SubCategory.Trim() : category.ToString(),
            Price = dto.Price >= 0 ? dto.Price : 0,
            CompareAtPrice = dto.CompareAtPrice,
            WarehouseStock = dto.WarehouseStock >= 0 ? dto.WarehouseStock : 0,
            ShortDescription = dto.ShortDescription ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            IsAvailable = dto.IsAvailable,
            Images = images
        };

        var created = await _productRepository.CreateAsync(product);

        if (_warehouseRepository != null)
        {
            try
            {
                var inventory = new WarehouseInventory
                {
                    ProductId = created.Id,
                    Sku = created.Sku,
                    ProductTitle = created.Title,
                    Category = created.Category,
                    TotalStock = created.WarehouseStock,
                    AvailableStock = created.WarehouseStock,
                    ReservedStock = 0,
                    LowStockThreshold = created.LowStockThreshold,
                    IsLowStock = created.WarehouseStock <= created.LowStockThreshold,
                    AdjustmentLog = new List<StockAdjustmentLogEntry>
                    {
                        new()
                        {
                            AdjustedBy = "SYSTEM",
                            AdjustedByName = "Initial Product Creation",
                            AdjustedByRole = "Admin",
                            PreviousStock = 0,
                            NewStock = created.WarehouseStock,
                            ChangeAmount = created.WarehouseStock,
                            Reason = "Initial inventory intake upon product creation",
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };
                await _warehouseRepository.CreateAsync(inventory);
            }
            catch
            {
                // Non-blocking for product creation
            }
        }

        return created;
    }

    public async Task<Product?> UpdateAsync(string id, UpdateProductDto dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title)) existing.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Sku)) existing.Sku = dto.Sku.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ShortDescription)) existing.ShortDescription = dto.ShortDescription.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Description)) existing.Description = dto.Description.Trim();
        if (dto.Price >= 0) existing.Price = dto.Price;
        if (dto.WarehouseStock >= 0) existing.WarehouseStock = dto.WarehouseStock;

        if (!string.IsNullOrWhiteSpace(dto.Category))
        {
            existing.Category = ParseProductCategory(dto.Category);
        }

        if (dto.Images != null)
        {
            existing.Images = dto.Images;
        }

        await _productRepository.UpdateAsync(existing);
        return existing;
    }

    public Task AddImageAsync(string id, string url) => Task.CompletedTask;
}
