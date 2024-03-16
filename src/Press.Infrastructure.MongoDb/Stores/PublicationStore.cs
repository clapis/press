using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

internal class PublicationStore(IMongoCollection<Publication> publications) : IPublicationStore
{
    public async Task<List<string>> GetAllUrlsAsync(CancellationToken cancellationToken) 
        => await publications.Find(x => true)
            .Project(x => x.Url)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Publication publication, CancellationToken cancellationToken) 
        => await publications.InsertOneAsync(publication, cancellationToken: cancellationToken);

    public Task<List<Publication>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var search = new BsonDocumentPipelineStageDefinition<Publication, Publication>(new BsonDocument
        {
            {
                "$search",
                new BsonDocument { { "phrase", new BsonDocument { { "query", query }, { "path", "Contents" } } } }
            }
        });

        return publications.Aggregate()
            .AppendStage(search)
            .SortByDescending(x => x.Date)
            .Limit(15)
            .Project<Publication>(Builders<Publication>.Projection.Exclude(f => f.Contents))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Publication>> GetLatestPublicationsBySourceAsync(CancellationToken cancellationToken)
    {
        return publications
            .AsQueryable()
            .OrderByDescending(x => x.Date)
            .GroupBy(x => x.Source)
            .Select(g => new
            {
                Source = g.Key,
                LastPublication = g.First()
            })
            .Select(x => x.LastPublication)
            .ToListAsync(cancellationToken);
    }
}