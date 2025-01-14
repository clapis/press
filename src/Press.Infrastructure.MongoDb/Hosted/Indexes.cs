using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Press.Core.Domain;

namespace Press.Infrastructure.MongoDb.Hosted;

internal class Indexes(
    IMongoCollection<User> users,
    IMongoCollection<Alert> alerts,
    IMongoCollection<Publication> publications) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.CustomerId),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);
        
        await alerts.Indexes.CreateOneAsync(new CreateIndexModel<Alert>(
            Builders<Alert>.IndexKeys.Ascending(x => x.UserId),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);

        await publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys.Descending(x => x.Date),
            new CreateIndexOptions { Background = true, ExpireAfter = TimeSpan.FromDays(90)}), cancellationToken: cancellationToken);

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