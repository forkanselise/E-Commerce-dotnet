using NexusBakery.Application.DTOs.Warehouse;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Application.Services;

public interface IWarehouseService
{
    Task<List<WarehouseInventoryDto>> GetAllInventoryAsync(CancellationToken cancellationToken = default);
    Task<List<WarehouseInventoryDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<WarehouseInventoryDto?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<bool> AdjustStockAsync(string productId, int newStock, string adjustedBy, string adjustedByName, string adjustedByRole, string reason, CancellationToken cancellationToken = default);
}

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository, IProductRepository productRepository)
    {
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
    }

    public async Task<List<WarehouseInventoryDto>> GetAllInventoryAsync(CancellationToken cancellationToken = default)
    {
        var items = await _warehouseRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<List<WarehouseInventoryDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _warehouseRepository.GetLowStockItemsAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<WarehouseInventoryDto?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        var item = await _warehouseRepository.GetByProductIdAsync(productId, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<bool> AdjustStockAsync(
        string productId,
        int newStock,
        string adjustedBy,
        string adjustedByName,
        string adjustedByRole,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var success = await _warehouseRepository.AdjustStockAsync(
            productId,
            newStock,
            adjustedBy,
            adjustedByName,
            adjustedByRole,
            reason,
            cancellationToken);

        if (success)
        {
            // Sync with Product read-model
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                product.WarehouseStock = newStock;
                product.IsAvailable = newStock > 0;
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
        }

        return success;
    }

    private static WarehouseInventoryDto MapToDto(WarehouseInventory w)
    {
        return new WarehouseInventoryDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            Sku = w.Sku,
            ProductTitle = w.ProductTitle,
            Category = w.Category,
            TotalStock = w.TotalStock,
            AvailableStock = w.AvailableStock,
            ReservedStock = w.ReservedStock,
            LowStockThreshold = w.LowStockThreshold,
            IsLowStock = w.IsLowStock,
            Location = w.Location,
            AdjustmentLog = w.AdjustmentLog,
            LastAuditAt = w.LastAuditAt,
            UpdatedAt = w.UpdatedAt
        };
    }
}
