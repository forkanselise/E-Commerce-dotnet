using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class ProductRepository : MongoRepository<Product>, IProductRepository
{
    public ProductRepository(MongoDbContext dbContext) : base(dbContext.Products)
    {
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Sku == sku).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Product>> GetFeaturedAsync(int limit = 8, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.IsFeatured && x.IsAvailable)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Product> Items, long TotalCount)> SearchProductsAsync(
        string? searchTerm,
        ProductCategory? category,
        string? subCategory,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStockOnly,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Eq(x => x.IsAvailable, true);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = new BsonRegularExpression(Regex.Escape(searchTerm.Trim()), "i");
            var titleOrDescFilter = builder.Or(
                builder.Regex(x => x.Title, pattern),
                builder.Regex(x => x.Description, pattern),
                builder.Regex(x => x.Tags, pattern)
            );
            filter = builder.And(filter, titleOrDescFilter);
        }

        if (category.HasValue)
        {
            filter = builder.And(filter, builder.Eq(x => x.Category, category.Value));
        }

        if (!string.IsNullOrWhiteSpace(subCategory))
        {
            filter = builder.And(filter, builder.Eq(x => x.SubCategory, subCategory.Trim()));
        }

        if (minPrice.HasValue)
        {
            filter = builder.And(filter, builder.Gte(x => x.Price, minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            filter = builder.And(filter, builder.Lte(x => x.Price, maxPrice.Value));
        }

        if (inStockOnly.HasValue && inStockOnly.Value)
        {
            filter = builder.And(filter, builder.Gt(x => x.WarehouseStock, 0));
        }

        var totalCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var query = Collection.Find(filter);

        query = sortBy?.ToLowerInvariant() switch
        {
            "price_asc" => query.SortBy(x => x.Price),
            "price_desc" => query.SortByDescending(x => x.Price),
            "rating" => query.SortByDescending(x => x.AverageRating),
            "newest" => query.SortByDescending(x => x.CreatedAt),
            _ => query.SortByDescending(x => x.IsFeatured).ThenByDescending(x => x.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> UpdateStockAsync(string productId, int quantityChange, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(x => x.Id, productId);
        var update = Builders<Product>.Update
            .Inc(x => x.WarehouseStock, quantityChange)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}
