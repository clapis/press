using MongoDB.Driver;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

public class UserStore(IMongoCollection<User> users) : IUserStore
{
    public async Task InsertAsync(User user, CancellationToken cancellationToken)
        => await users.InsertOneAsync(user, cancellationToken: cancellationToken);
}