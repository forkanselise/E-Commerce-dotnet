using System.Linq.Expressions;
using NexusBakery.Domain.Common;

namespace NexusBakery.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<(List<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int page,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<Entities.User>
{
    Task<Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<bool> UpdateRefreshTokenAsync(string userId, Entities.RefreshTokenRecord refreshToken, CancellationToken cancellationToken = default);
}

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<Entities.Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Entities.Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<List<Entities.Product>> GetFeaturedAsync(int limit = 8, CancellationToken cancellationToken = default);
    Task<(List<Entities.Product> Items, long TotalCount)> SearchProductsAsync(
        string? searchTerm,
        Enums.ProductCategory? category,
        string? subCategory,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStockOnly,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(string productId, int quantityChange, CancellationToken cancellationToken = default);
}

public interface ITutorialRepository : IRepository<Entities.Tutorial>
{
    Task<Entities.Tutorial?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Entities.Tutorial>> GetPopularAsync(int limit = 6, CancellationToken cancellationToken = default);
    Task<(List<Entities.Tutorial> Items, long TotalCount)> SearchTutorialsAsync(
        string? searchTerm,
        string? category,
        Enums.SkillLevel? skillLevel,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(string tutorialId, CancellationToken cancellationToken = default);
}

public interface IOrderRepository : IRepository<Entities.Order>
{
    Task<Entities.Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<List<Entities.Order>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(string orderId, Enums.OrderStatus newStatus, string? note = null, CancellationToken cancellationToken = default);
    Task<bool> UpdatePaymentStatusAsync(string orderId, Enums.PaymentStatus paymentStatus, string? transactionId = null, CancellationToken cancellationToken = default);
}

public interface IWarehouseRepository : IRepository<Entities.WarehouseInventory>
{
    Task<Entities.WarehouseInventory?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<Entities.WarehouseInventory?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<List<Entities.WarehouseInventory>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<bool> AdjustStockAsync(string productId, int newStock, string adjustedBy, string adjustedByName, string adjustedByRole, string reason, CancellationToken cancellationToken = default);
}

public interface IMetricsRepository : IRepository<Entities.SiteMetrics>
{
    Task<Entities.SiteMetrics> GetGlobalMetricsAsync(CancellationToken cancellationToken = default);
    Task<int> IncrementLiveUsersAsync(int delta, CancellationToken cancellationToken = default);
    Task IncrementLifetimeVisitsAsync(CancellationToken cancellationToken = default);
    Task RecordOrderCompletedAsync(decimal revenue, CancellationToken cancellationToken = default);
}
