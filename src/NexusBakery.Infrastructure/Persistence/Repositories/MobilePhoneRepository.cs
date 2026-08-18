using NexusBakery.Domain.Entities;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Infrastructure.Persistence.Repositories;

public class MobilePhoneRepository : MongoRepository<MobilePhone>, IMobilePhoneRepository
{
    public MobilePhoneRepository(MongoDbContext context) 
        : base(context.MobilePhones)
    {
    }
}
