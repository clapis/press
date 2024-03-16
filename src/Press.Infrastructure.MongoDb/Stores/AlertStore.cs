using MongoDB.Driver;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Infrastructure.MongoDb.Stores;

internal class AlertStore(IMongoCollection<Alert> alerts) : IAlertStore
{
    public async Task<List<Alert>> GetAllAsync(CancellationToken cancellationToken) 
        => await alerts.Find(x => true).ToListAsync(cancellationToken);

    public async Task InsertAsync(Alert alert, CancellationToken cancellationToken) 
        => await alerts.InsertOneAsync(alert, cancellationToken: cancellationToken);

    public async Task UpdateAsync(Alert alert, CancellationToken cancellationToken)
    {
        var filter = Builders<Alert>.Filter.Eq(x => x.Id, alert.Id);

        var update = Builders<Alert>.Update.Set(x => x.LastNotification, alert.LastNotification);

        await alerts.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, cancellationToken);
    }
}