using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Domain.Entities;

public class WarehouseInventory : BaseEntity
{
    [BsonElement("productId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("productTitle")]
    public string ProductTitle { get; set; } = string.Empty;

    [BsonElement("category")]
    public ProductCategory Category { get; set; } = ProductCategory.Bakery;

    [BsonElement("totalStock")]
    public int TotalStock { get; set; } = 0;

    [BsonElement("reservedStock")]
    public int ReservedStock { get; set; } = 0;

    [BsonElement("availableStock")]
    public int AvailableStock { get; set; } = 0;

    [BsonElement("lowStockThreshold")]
    public int LowStockThreshold { get; set; } = 5;

    [BsonElement("reorderPoint")]
    public int ReorderPoint { get; set; } = 10;

    [BsonElement("isLowStock")]
    public bool IsLowStock { get; set; } = false;

    [BsonElement("location")]
    public WarehouseLocation? Location { get; set; }

    [BsonElement("adjustmentLog")]
    public List<StockAdjustmentLogEntry> AdjustmentLog { get; set; } = new();

    [BsonElement("lastAuditAt")]
    public DateTime? LastAuditAt { get; set; }
}

public class WarehouseLocation
{
    [BsonElement("warehouse")]
    public string Warehouse { get; set; } = "Dhaka Central Warehouse";

    [BsonElement("zone")]
    public string Zone { get; set; } = "A";

    [BsonElement("shelf")]
    public string Shelf { get; set; } = "A-01";

    [BsonElement("bin")]
    public string Bin { get; set; } = "A-01-01";
}

public class StockAdjustmentLogEntry
{
    [BsonElement("adjustedBy")]
    public string AdjustedBy { get; set; } = string.Empty;

    [BsonElement("adjustedByName")]
    public string AdjustedByName { get; set; } = string.Empty;

    [BsonElement("adjustedByRole")]
    public string AdjustedByRole { get; set; } = "Admin";

    [BsonElement("previousStock")]
    public int PreviousStock { get; set; }

    [BsonElement("newStock")]
    public int NewStock { get; set; }

    [BsonElement("changeAmount")]
    public int ChangeAmount { get; set; }

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
