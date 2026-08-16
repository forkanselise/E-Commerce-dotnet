using System.Linq.Expressions;
using MongoDB.Driver;
using NexusBakery.Domain.Common;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection;

    public MongoRepository(IMongoCollection<T> collection)
    {
        Collection = collection;
    }

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<(List<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int page,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default)
    {
        var filter = predicate ?? (_ => true);
        var totalCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var query = Collection.Find(filter);
        if (orderBy != null)
        {
            query = isDescending ? query.SortByDescending(orderBy) : query.SortBy(orderBy);
        }
        else
        {
            query = query.SortByDescending(x => x.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await Collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public virtual async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var filter = predicate ?? (_ => true);
        return await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
