using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Press.Core.Publications;

namespace Press.MongoDb;

internal class Indexes : IHostedService
{
    private readonly IMongoCollection<Publication> _publications;

    public Indexes(IMongoCollection<Publication> publications) => _publications = publications;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys.Descending(x => x.Date),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}