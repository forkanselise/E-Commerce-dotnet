using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Application.DTOs.Products;

public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string SubCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "BDT";
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductSpecification> Specifications { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string Sku { get; set; } = string.Empty;
    public int WarehouseStock { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductSearchRequest
{
    public string? Search { get; set; }
    public ProductCategory? Category { get; set; }
    public string? SubCategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public string? Sort { get; set; } // "price_asc", "price_desc", "rating", "newest"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string SubCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int InitialStock { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5;
    public bool IsFeatured { get; set; } = false;
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductSpecification> Specifications { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class UpdateProductRequest
{
    public string? Title { get; set; }
    public ProductCategory? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsFeatured { get; set; }
    public List<ProductImage>? Images { get; set; }
    public List<ProductSpecification>? Specifications { get; set; }
    public List<string>? Tags { get; set; }
}
