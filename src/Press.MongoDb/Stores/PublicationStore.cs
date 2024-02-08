using MongoDB.Bson;
using MongoDB.Driver;
using Press.Core.Publications;

namespace Press.MongoDb.Stores
{
    internal class PublicationStore : IPublicationStore
    {
        private readonly IMongoCollection<Publication> _publications;

        public PublicationStore(IMongoCollection<Publication> publications) => _publications = publications;

        public async Task<DateTime> GetMaxDateAsync(CancellationToken cancellationToken)
        {
            return await _publications.Find(x => true)
                .SortByDescending(x => x.Date)
                .Limit(1)
                .Project(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<string>> GetAllUrlsAsync(CancellationToken cancellationToken)
        {
            return await _publications.Find(x => true)
                .Project(x => x.Url)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveAsync(Publication publication, CancellationToken cancellationToken)
        {
            await _publications.InsertOneAsync(publication, cancellationToken: cancellationToken);
        }

        public Task<List<Publication>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            var search = new BsonDocumentPipelineStageDefinition<Publication, Publication>(new BsonDocument
            {
                { "$search", new BsonDocument { { "phrase", new BsonDocument { { "query", query }, { "path", "Contents" } } } } }
            });

            return _publications.Aggregate()
                .AppendStage(search)
                .SortByDescending(x => x.Date)
                .Limit(15)
                .Project<Publication>(Builders<Publication>.Projection.Exclude(f => f.Contents))
                .ToListAsync(cancellationToken);
        }
    }
}