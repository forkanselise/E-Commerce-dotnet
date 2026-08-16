using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Enums;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class TutorialRepository : MongoRepository<Tutorial>, ITutorialRepository
{
    public TutorialRepository(MongoDbContext dbContext) : base(dbContext.Tutorials)
    {
    }

    public async Task<Tutorial?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetPopularAsync(int limit = 6, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.IsPublished)
            .SortByDescending(x => x.ViewCount)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Tutorial> Items, long TotalCount)> SearchTutorialsAsync(
        string? searchTerm,
        string? category,
        SkillLevel? skillLevel,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<Tutorial>.Filter;
        var filter = builder.Eq(x => x.IsPublished, true);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = new BsonRegularExpression(Regex.Escape(searchTerm.Trim()), "i");
            var textFilter = builder.Or(
                builder.Regex(x => x.Title, pattern),
                builder.Regex(x => x.Description, pattern),
                builder.Regex(x => x.Tags, pattern)
            );
            filter = builder.And(filter, textFilter);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter = builder.And(filter, builder.Eq(x => x.Category, category.Trim()));
        }

        if (skillLevel.HasValue)
        {
            filter = builder.And(filter, builder.Eq(x => x.SkillLevel, skillLevel.Value));
        }

        var totalCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await Collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task IncrementViewCountAsync(string tutorialId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Tutorial>.Filter.Eq(x => x.Id, tutorialId);
        var update = Builders<Tutorial>.Update.Inc(x => x.ViewCount, 1);
        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
