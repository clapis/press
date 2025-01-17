using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

internal class PublicationStore(IMongoCollection<Publication> publications) : IPublicationStore
{
    public async Task<List<string>> GetLatestUrlsAsync(Source source, CancellationToken cancellationToken) 
        => await publications.Find(x => x.SourceId == source.Id)
            .SortByDescending(x => x.Date)
            .Limit(500)
            .Project(x => x.Url)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Publication publication, CancellationToken cancellationToken) 
        => await publications.InsertOneAsync(publication, cancellationToken: cancellationToken);

    public Task<List<Publication>> SearchAsync(string query, string? sourceId, CancellationToken cancellationToken)
    {
        var search = publications.Aggregate()
            .Search(Builders<Publication>.Search.Phrase(x => x.Contents, query))
            .Project<Publication>(Builders<Publication>.Projection.Exclude(f => f.Contents));
        
        if (sourceId != null)
            search = search.Match(x => x.SourceId == sourceId);
            
        return search
            .SortByDescending(x => x.Date)
            .Limit(10)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Publication>> GetLatestBySourceAsync(CancellationToken cancellationToken)
    {
        return publications
            .AsQueryable()
            .OrderByDescending(x => x.Date)
            .GroupBy(x => x.SourceId)
            .Select(g => g.First())
            .Select(x => new Publication
            {
                Id = x.Id,
                Url = x.Url,
                Date = x.Date,
                SourceId = x.SourceId,
                CreatedOn = x.CreatedOn
            })
            .OrderBy(x => x.SourceId)
            .ToListAsync(cancellationToken);
    }
}