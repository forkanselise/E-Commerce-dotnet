namespace NexusBakery.Application.DTOs.Products;

public class UpdateProductDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int WarehouseStock { get; set; }
    public bool? IsAvailable { get; set; }
    public System.Collections.Generic.List<NexusBakery.Domain.Entities.ProductImage>? Images { get; set; }
}
