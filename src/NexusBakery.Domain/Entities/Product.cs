using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class Product : BaseEntity
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("category")]
    public ProductCategory Category { get; set; } = ProductCategory.Bakery;

    [BsonElement("subCategory")]
    public string SubCategory { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("shortDescription")]
    public string ShortDescription { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("compareAtPrice")]
    public decimal? CompareAtPrice { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";

    [BsonElement("images")]
    public List<ProductImage> Images { get; set; } = new();

    [BsonElement("specifications")]
    public List<ProductSpecification> Specifications { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("warehouseStock")]
    public int WarehouseStock { get; set; } = 0;

    [BsonElement("lowStockThreshold")]
    public int LowStockThreshold { get; set; } = 5;

    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    [BsonElement("isFeatured")]
    public bool IsFeatured { get; set; } = false;

    [BsonElement("averageRating")]
    public double AverageRating { get; set; } = 5.0;

    [BsonElement("reviewCount")]
    public int ReviewCount { get; set; } = 0;

    [BsonElement("weight")]
    public double? Weight { get; set; }

    [BsonElement("relatedProductIds")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> RelatedProductIds { get; set; } = new();

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CreatedBy { get; set; }

    [BsonElement("updatedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UpdatedBy { get; set; }
}

public class ProductImage
{
    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("alt")]
    public string Alt { get; set; } = string.Empty;

    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; } = false;
}

public class ProductSpecification
{
    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
}
