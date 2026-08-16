using MongoDB.Driver;
using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext dbContext) : base(dbContext.Users)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await Collection.Find(x => x.Email.ToLower() == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.Phone == phone).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(x => x.GoogleId == googleId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateRefreshTokenAsync(string userId, RefreshTokenRecord refreshToken, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, userId);
        var update = Builders<User>.Update
            .Push(x => x.RefreshTokens, refreshToken)
            .Set(x => x.LastLoginAt, DateTime.UtcNow)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}
