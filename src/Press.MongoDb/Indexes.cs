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
        await EnsurePublicationSource();
        
        await _publications.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(
            Builders<Publication>.IndexKeys.Descending(x => x.Date),
            new CreateIndexOptions { Background = true }), cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsurePublicationSource()
    {
        var docs = await _publications.Find(Builders<Publication>.Filter.Exists(x => x.Source, false)).ToListAsync();

        foreach (var doc in docs)
        {
            doc.Source = PublicationSource.Unknown;
            
            if (doc.Url.StartsWith("http://noticias.sorocaba.sp.gov.br"))
                doc.Source = PublicationSource.Sorocaba;

            if (doc.Url.StartsWith("https://www.franca.sp.gov.br"))
                doc.Source = PublicationSource.Franca;

            if (doc.Url.StartsWith("http://www.saocarlos.sp.gov.br"))
                doc.Source = PublicationSource.SaoCarlos;
            
            await _publications.ReplaceOneAsync(x => x.Id == doc.Id, doc);
        }
    }
}