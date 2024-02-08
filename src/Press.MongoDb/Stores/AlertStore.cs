using MongoDB.Driver;
using Press.Core.Alerts;

namespace Press.MongoDb.Stores
{
    internal class AlertStore : IAlertStore
    {
        private readonly IMongoCollection<Alert> _alerts;

        public AlertStore(IMongoCollection<Alert> alerts) => _alerts = alerts;

        public async Task<List<Alert>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _alerts.Find(x => true).ToListAsync(cancellationToken);
        }

        public async Task InsertAsync(Alert alert, CancellationToken cancellationToken)
        {
            await _alerts.InsertOneAsync(alert, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Alert alert, CancellationToken cancellationToken)
        {
            var filter = Builders<Alert>.Filter.Eq(x => x.Id, alert.Id);

            var update = Builders<Alert>.Update.Set(x => x.LastNotification, alert.LastNotification);

            await _alerts.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, cancellationToken);
        }
    }
}