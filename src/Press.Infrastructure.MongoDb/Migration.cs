using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Press.Core.Domain;

namespace Press.Infrastructure.MongoDb;

public class Migration(IMongoCollection<Source> sources, IMongoCollection<Publication> publications, ILogger<Migration> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        
        var update = Builders<Publication>.Update.Rename("Source", "SourceId");

        await publications.UpdateManyAsync(x => true, update, cancellationToken: cancellationToken);

        await publications.UpdateManyAsync(x => x.SourceId == "Franca",
            Builders<Publication>.Update.Set(x => x.SourceId, "dom_sp_franca"),
            cancellationToken: cancellationToken);

        await publications.UpdateManyAsync(x => x.SourceId == "Sorocaba",
            Builders<Publication>.Update.Set(x => x.SourceId, "dom_sp_sorocaba"),
            cancellationToken: cancellationToken);

        await publications.UpdateManyAsync(x => x.SourceId == "SaoCarlos",
            Builders<Publication>.Update.Set(x => x.SourceId, "dom_sp_sao_carlos"),
            cancellationToken: cancellationToken);

        var srcs = new Source[]
        {
            new () { Id = "dom_sp_franca", Name = "Franca - SP", Url = "https://www.franca.sp.gov.br/pmf-diario/", IsEnabled = true },
            new () { Id = "dom_sp_sorocaba", Name = "Sorocaba - SP", Url = "https://noticias.sorocaba.sp.gov.br/jornal/", IsEnabled = true },
            new () { Id = "dom_sp_sao_carlos", Name = "São Carlos - SP", Url = "http://www.saocarlos.sp.gov.br/index.php/diario-oficial.html", IsEnabled = true }
        };
        
        foreach(var src in srcs)
            await sources.ReplaceOneAsync(x => x.Id == src.Id, src, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        
        logger.LogInformation($"Migration finished in {sw.Elapsed}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}