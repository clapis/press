using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Providers.SP.Franca;

public class PublicationScraper(
    HttpClient httpClient,
    IPdfContentExtractor extractor) : IPublicationScraper
{
    public string SourceId => "dom_sp_franca";
    
    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var dates = Enumerable.Range(0, 7)
            .Select(x => DateTime.UtcNow.Date.AddDays(-x))
            .Reverse();

        foreach (var date in dates)
        {
            var links = await GetPublicationsByDateAsync(date, cancellationToken);

            foreach (var link in links.Except(existing))
            {
                using var file = await httpClient.DownloadAsync(link, cancellationToken);
                
                var contents = await extractor.ExtractTextAsync(file.Path, cancellationToken);
                
                yield return new Publication
                {
                    Url = link,
                    SourceId = SourceId,
                    Contents = contents,
                    Date = date
                };
            }
        }
    }

    private async Task<IEnumerable<string>> GetPublicationsByDateAsync(DateTime date, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(BuildPublicationsPageUrl(date), cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(content)) return [];

        return JsonSerializer.Deserialize<List<Entry>>(content)!
            .Select(x => BuildPublicationLink(x.Filename));
    }

    private static string BuildPublicationsPageUrl(DateTime date) 
        => $"https://www.franca.sp.gov.br/pmf-diario/rest/diario/buscaPorArquivo/{date:dd-MM-yyyy}";

    private static string BuildPublicationLink(string filename) 
        => $"https://www.franca.sp.gov.br/arquivos/diario-oficial/documentos/{filename}";

    private record Entry([property: JsonPropertyName("nome")] string Filename);
}