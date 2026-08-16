using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class OrderRepository : MongoRepository<Order>, IOrderRepository
{
    public OrderRepository(MongoDbContext dbContext) : base(dbContext.Orders)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.OrderNumber == orderNumber).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(string orderId, OrderStatus newStatus, string? note = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, orderId);
        var historyItem = new OrderStatusHistoryItem
        {
            Status = newStatus,
            Timestamp = DateTime.UtcNow,
            Note = note
        };

        var update = Builders<Order>.Update
            .Set(x => x.OrderStatus, newStatus)
            .Push(x => x.StatusHistory, historyItem)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> UpdatePaymentStatusAsync(string orderId, PaymentStatus paymentStatus, string? transactionId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, orderId);
        var update = Builders<Order>.Update
            .Set(x => x.PaymentStatus, paymentStatus)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(transactionId))
        {
            update = update.Set(x => x.PaymentDetails.TransactionId, transactionId)
                           .Set(x => x.PaymentDetails.PaidAt, DateTime.UtcNow);
        }

        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}
