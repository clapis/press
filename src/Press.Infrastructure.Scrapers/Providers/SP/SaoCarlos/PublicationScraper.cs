using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Providers.SP.SaoCarlos;

public class PublicationScraper(
    HttpClient httpClient,
    IPdfContentExtractor extractor
    ) : IPublicationScraper
{
    public string SourceId => "dom_sp_sao_carlos";
    
    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pubs = await GetPublicationsAsync(cancellationToken);

        foreach (var pub in pubs.ExceptBy(existing, x => x.Url))
        {
            using var file = await httpClient.DownloadAsync(pub.Url, cancellationToken);
            
            var contents = await extractor.ExtractTextAsync(file.Path, cancellationToken);

            yield return new Publication
            {
                Url = pub.Url,
                SourceId = SourceId,
                Contents = contents,
                Date = pub.Start
            };
        }
    }

    private async Task<List<Pub>> GetPublicationsAsync(CancellationToken cancellationToken)
    {
        var offset = TimeSpan.FromDays(45);
        
        var url = $"https://cidadao.saocarlos.sp.gov.br/servicos/jornal/include/publicacoes.php?calendario=true&permissao=0&start={DateTime.UtcNow.Add(offset):s}&end={DateTime.UtcNow:s}";

        return await httpClient.GetFromJsonAsync<List<Pub>>(url, cancellationToken) ??
               throw new Exception("Failed to retrieve publications");
    }

    private record Pub(DateTime Start, string Description)
    {
        public string Url { get; } = $"https://cidadao.saocarlos.sp.gov.br/servicos/jornal/{Description}";
    }
}