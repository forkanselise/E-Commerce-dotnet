using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;

namespace NexusBakery.Application.DTOs.Warehouse;

public class WarehouseInventoryDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public int TotalStock { get; set; }
    public int AvailableStock { get; set; }
    public int ReservedStock { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public WarehouseLocation? Location { get; set; }
    public List<StockAdjustmentLogEntry> AdjustmentLog { get; set; } = new();
    public DateTime? LastAuditAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdjustStockRequest
{
    public int NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class BatchAdjustStockRequest
{
    public List<BatchStockItem> Adjustments { get; set; } = new();
}

public class BatchStockItem
{
    public string ProductId { get; set; } = string.Empty;
    public int NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
}
