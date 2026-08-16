using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class MetricsRepository : MongoRepository<SiteMetrics>, IMetricsRepository
{
    private const string GlobalMetricsId = "000000000000000000000001";

    public MetricsRepository(MongoDbContext dbContext) : base(dbContext.SiteMetrics)
    {
    }

    public async Task<SiteMetrics> GetGlobalMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await Collection.Find(x => x.Id == GlobalMetricsId).FirstOrDefaultAsync(cancellationToken);
        if (metrics == null)
        {
            metrics = new SiteMetrics
            {
                Id = GlobalMetricsId,
                TotalLifetimeVisits = 1,
                ActiveLiveUsers = 1,
                TotalRegisteredUsers = 0,
                TotalOrdersCompleted = 0,
                TotalRevenue = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await Collection.InsertOneAsync(metrics, cancellationToken: cancellationToken);
        }
        return metrics;
    }

    public async Task<int> IncrementLiveUsersAsync(int delta, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SiteMetrics>.Filter.Eq(x => x.Id, GlobalMetricsId);
        var update = Builders<SiteMetrics>.Update
            .Inc(x => x.ActiveLiveUsers, delta)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<SiteMetrics>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var result = await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        var count = Math.Max(1, result?.ActiveLiveUsers ?? 1);
        return count;
    }

    public async Task IncrementLifetimeVisitsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var filter = Builders<SiteMetrics>.Filter.Eq(x => x.Id, GlobalMetricsId);
        var update = Builders<SiteMetrics>.Update
            .Inc(x => x.TotalLifetimeVisits, 1)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await Collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task RecordOrderCompletedAsync(decimal revenue, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SiteMetrics>.Filter.Eq(x => x.Id, GlobalMetricsId);
        var update = Builders<SiteMetrics>.Update
            .Inc(x => x.TotalOrdersCompleted, 1)
            .Inc(x => x.TotalRevenue, revenue)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await Collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
    }
}
