using System.Text.Json;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Franca;

public class SourcePublicationProvider(HttpClient httpClient) : ISourcePublicationProvider
{
    public string SourceId => "dom_sp_franca";

    public async Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken)
    {
        var publications = new List<Publication>();
        
        foreach (var date in Last7Days())
        {
            var links = await GetPublicationsByDateAsync(date, cancellationToken);

            publications.AddRange(links
                .Select(link => new Publication
                {
                    Url = link,
                    SourceId = SourceId,
                    Date = DateTime.UtcNow.Date
                }));
        }

        return publications;
    }

    private async Task<IEnumerable<string>> GetPublicationsByDateAsync(DateTime date, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(BuildPublicationsPageUrl(date), cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(content)) return Enumerable.Empty<string>();

        return JsonSerializer.Deserialize<List<Entry>>(content)!
            .Select(x => BuildPublicationLink(x.nome));
    }

    private static IEnumerable<DateTime> Last7Days() 
        => Enumerable.Range(0, 7).Select(x => DateTime.Today.AddDays(-x));

    private static string BuildPublicationsPageUrl(DateTime date) 
        => $"https://www.franca.sp.gov.br/pmf-diario/rest/diario/buscaPorArquivo/{date:dd-MM-yyyy}";

    private static string BuildPublicationLink(string filename) 
        => $"https://www.franca.sp.gov.br/arquivos/diario-oficial/documentos/{filename}";

    private record Entry(string nome);
}