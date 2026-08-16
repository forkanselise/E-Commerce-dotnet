using System.Text.RegularExpressions;
using NexusBakery.Application.DTOs.Products;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> GetFeaturedProductsAsync(int limit = 8, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, string createdByUserId, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateProductAsync(string id, UpdateProductRequest request, string updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(string id, CancellationToken cancellationToken = default);
    Task<List<CategoryGroupDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

public class CategoryGroupDto
{
    public ProductCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> SubCategories { get; set; } = new();
    public int ProductCount { get; set; }
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public ProductService(IProductRepository productRepository, IWarehouseRepository warehouseRepository)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _productRepository.SearchProductsAsync(
            request.Search,
            request.Category,
            request.SubCategory,
            request.MinPrice,
            request.MaxPrice,
            request.InStockOnly,
            request.Sort,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<ProductDto>> GetFeaturedProductsAsync(int limit = 8, CancellationToken cancellationToken = default)
    {
        var items = await _productRepository.GetFeaturedAsync(limit, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySlugAsync(slug, cancellationToken);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, string createdByUserId, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(request.Title);
        var existingSlug = await _productRepository.GetBySlugAsync(slug, cancellationToken);
        if (existingSlug != null)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        var product = new Product
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Category = request.Category,
            SubCategory = request.SubCategory.Trim(),
            Description = request.Description.Trim(),
            ShortDescription = request.ShortDescription.Trim(),
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Sku = string.IsNullOrWhiteSpace(request.Sku) ? $"SKU-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}" : request.Sku.Trim(),
            WarehouseStock = request.InitialStock,
            LowStockThreshold = request.LowStockThreshold,
            IsAvailable = true,
            IsFeatured = request.IsFeatured,
            Images = request.Images,
            Specifications = request.Specifications,
            Tags = request.Tags,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        var created = await _productRepository.CreateAsync(product, cancellationToken);

        // Also create initial warehouse inventory record
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
                    AdjustedBy = createdByUserId,
                    AdjustedByName = "Admin",
                    AdjustedByRole = "Admin",
                    PreviousStock = 0,
                    NewStock = created.WarehouseStock,
                    ChangeAmount = created.WarehouseStock,
                    Reason = "Product created",
                    Timestamp = DateTime.UtcNow
                }
            }
        };
        await _warehouseRepository.CreateAsync(inventory, cancellationToken);

        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(string id, UpdateProductRequest request, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Title)) product.Title = request.Title.Trim();
        if (request.Category.HasValue) product.Category = request.Category.Value;
        if (!string.IsNullOrWhiteSpace(request.SubCategory)) product.SubCategory = request.SubCategory.Trim();
        if (!string.IsNullOrWhiteSpace(request.Description)) product.Description = request.Description.Trim();
        if (!string.IsNullOrWhiteSpace(request.ShortDescription)) product.ShortDescription = request.ShortDescription.Trim();
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.CompareAtPrice.HasValue) product.CompareAtPrice = request.CompareAtPrice.Value;
        if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;
        if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;
        if (request.Images != null) product.Images = request.Images;
        if (request.Specifications != null) product.Specifications = request.Specifications;
        if (request.Tags != null) product.Tags = request.Tags;
        product.UpdatedBy = updatedByUserId;

        await _productRepository.UpdateAsync(product, cancellationToken);
        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null) return false;

        product.IsAvailable = false;
        return await _productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task<List<CategoryGroupDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.FindAsync(x => x.IsAvailable, cancellationToken);

        return products
            .GroupBy(p => p.Category)
            .Select(g => new CategoryGroupDto
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                SubCategories = g.Select(x => x.SubCategory).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                ProductCount = g.Count()
            })
            .ToList();
    }

    private static string GenerateSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Category = p.Category,
            SubCategory = p.SubCategory,
            Description = p.Description,
            ShortDescription = p.ShortDescription,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Currency = p.Currency,
            Images = p.Images,
            Specifications = p.Specifications,
            Tags = p.Tags,
            Sku = p.Sku,
            WarehouseStock = p.WarehouseStock,
            IsAvailable = p.IsAvailable,
            IsFeatured = p.IsFeatured,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            CreatedAt = p.CreatedAt
        };
    }
}
