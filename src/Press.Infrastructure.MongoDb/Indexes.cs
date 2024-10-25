using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Press.Core.Domain;

namespace Press.Infrastructure.MongoDb;

internal class Indexes(IMongoCollection<Publication> publications) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys.Descending(x => x.Date),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);

        await publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys
                .Ascending(x => x.SourceId)
                .Descending(x => x.Date),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);

        await publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys.Ascending(x => x.Url),
            new CreateIndexOptions { Background = true, Unique = true }), cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}