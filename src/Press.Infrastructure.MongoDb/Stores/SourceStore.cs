using MongoDB.Driver;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

public class SourceStore(IMongoCollection<Source> sources) : ISourceStore
{
    public Task<List<Source>> GetAllAsync(CancellationToken cancellationToken)
        => sources.Find(_ => true).ToListAsync(cancellationToken);
}