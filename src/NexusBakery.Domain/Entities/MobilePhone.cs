using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;

namespace NexusBakery.Domain.Entities;

public class MobilePhone : BaseEntity
{
    [BsonElement("brand")]
    public string Brand { get; set; } = string.Empty;

    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [BsonElement("processor")]
    public string Processor { get; set; } = string.Empty;

    [BsonElement("ram")]
    public string Ram { get; set; } = string.Empty;

    [BsonElement("storage")]
    public string Storage { get; set; } = string.Empty;

    [BsonElement("battery")]
    public string Battery { get; set; } = string.Empty;

    [BsonElement("screenSize")]
    public string ScreenSize { get; set; } = string.Empty;

    [BsonElement("stock")]
    public int Stock { get; set; } = 0;
}
