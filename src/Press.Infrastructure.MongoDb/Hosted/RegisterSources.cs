using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Press.Core.Domain;

namespace Press.Infrastructure.MongoDb.Hosted;

public class RegisterSources(IMongoCollection<Source> collection) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sources = new Source[]
        {
            new() { Id = "dou", Name = "DOU", Url = "https://www.in.gov.br/leiturajornal", IsEnabled = false, IsOfficial = true },
            
            new() { Id = "dom_sp_franca", Name = "Franca - SP", Url = "https://www.franca.sp.gov.br/pmf-diario/", IsOfficial = true, IsEnabled = true},
            new() { Id = "dom_sp_ribeirao_preto", Name = "Ribeirão Preto - SP", Url = "https://cespro.com.br/visualizarDiarioOficial.php?cdMunicipio=9314", IsOfficial = true, IsEnabled = true},
            new() { Id = "dom_sp_sao_carlos", Name = "São Carlos - SP", Url = "http://www.saocarlos.sp.gov.br/index.php/diario-oficial.html", IsOfficial = true, IsEnabled = true },
            new() { Id = "dom_sp_sorocaba", Name = "Sorocaba - SP", Url = "https://noticias.sorocaba.sp.gov.br/jornal/", IsOfficial = true, IsEnabled = true },
            
            new() { Id = "x_do_concursos", Name = "D.O. Concursos", Url = "https://www.diariooficial.com.br/concursos-lista", IsOfficial = false, IsEnabled = true }
        };

        var updates = sources.Select(source => new ReplaceOneModel<Source>(
                new ExpressionFilterDefinition<Source>(x => x.Id == source.Id), source) { IsUpsert = true });

        await collection.BulkWriteAsync(updates, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}