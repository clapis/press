using MongoDB.Driver;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

public class UserStore(IMongoCollection<User> users) : IUserStore
{
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        => await users.Find(_ => true).ToListAsync(cancellationToken);
    
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await users.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    
    public async Task InsertAsync(User user, CancellationToken cancellationToken)
        => await users.InsertOneAsync(user, cancellationToken: cancellationToken);
}