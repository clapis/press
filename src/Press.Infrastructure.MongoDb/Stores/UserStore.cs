using MongoDB.Driver;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

public class UserStore(IMongoCollection<User> users) : IUserStore
{
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken)
        => await users.Find(x => x.Subscription != null).ToListAsync(cancellationToken);
    
    public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await users.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken)
        => await users.Find(x => x.CustomerId == customerId).FirstOrDefaultAsync(cancellationToken);

    public async Task InsertAsync(User user, CancellationToken cancellationToken)
        => await users.InsertOneAsync(user, cancellationToken: cancellationToken);

    public async Task UpdateSubscriptionAsync(string id, Subscription? subscription, CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, id);

        var update = Builders<User>.Update.Set(x => x.Subscription, subscription);

        await users.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, cancellationToken);
    }
    
    public async Task ExpireTrialsAsync(CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Ne(x => x.Subscription, null),
            Builders<User>.Filter.Eq(x => x.Subscription!.IsTrial, true),
            Builders<User>.Filter.Lt(x => x.CreatedOn, DateTime.UtcNow.AddDays(-15)));
        
        var update = Builders<User>.Update.Set(x => x.Subscription, null);

        await users.UpdateManyAsync(filter, update, new UpdateOptions { IsUpsert = false }, cancellationToken);
    }
}