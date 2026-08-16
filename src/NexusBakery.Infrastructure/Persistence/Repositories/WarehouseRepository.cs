using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class WarehouseRepository : MongoRepository<WarehouseInventory>, IWarehouseRepository
{
    public WarehouseRepository(MongoDbContext dbContext) : base(dbContext.WarehouseInventories)
    {
    }

    public async Task<WarehouseInventory?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WarehouseInventory?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Sku == sku).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WarehouseInventory>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.AvailableStock <= x.LowStockThreshold)
            .SortBy(x => x.AvailableStock)
            .ToListAsync(cancellationToken);
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
        var inventory = await GetByProductIdAsync(productId, cancellationToken);
        if (inventory == null) return false;

        var previousStock = inventory.AvailableStock;
        var change = newStock - previousStock;

        var logEntry = new StockAdjustmentLogEntry
        {
            AdjustedBy = adjustedBy,
            AdjustedByName = adjustedByName,
            AdjustedByRole = adjustedByRole,
            PreviousStock = previousStock,
            NewStock = newStock,
            ChangeAmount = change,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };

        var filter = Builders<WarehouseInventory>.Filter.Eq(x => x.ProductId, productId);
        var update = Builders<WarehouseInventory>.Update
            .Set(x => x.AvailableStock, newStock)
            .Set(x => x.TotalStock, newStock + inventory.ReservedStock)
            .Set(x => x.IsLowStock, newStock <= inventory.LowStockThreshold)
            .Set(x => x.LastAuditAt, DateTime.UtcNow)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Push(x => x.AdjustmentLog, logEntry);

        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}
