namespace NexusBakery.Application.DTOs.Products; 

public class CreateProductDto 
{ 
    public string? Title { get; set; } 
    public string? Slug { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; } 
    public decimal Price { get; set; } 
    public int WarehouseStock { get; set; } 
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
}
