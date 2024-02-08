using System.Runtime.CompilerServices;
using System.Text.Json;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Franca;

public class PublicationProvider(HttpClient httpClient) : IPublicationProvider
{
    public async IAsyncEnumerable<Publication> ProvideAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var date in Last15Days())
        {
            var links = await GetPublicationsByDateAsync(date, cancellationToken);

            foreach (var link in links)
                yield return new Publication
                {
                    Url = link,
                    Date = date,
                    Source = PublicationSource.Franca
                };
        }
    }

    private async Task<IEnumerable<string>> GetPublicationsByDateAsync(DateTime date,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(PublicationsByDate(date), cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(content)) return Enumerable.Empty<string>();

        return JsonSerializer.Deserialize<List<Entry>>(content)!
            .Select(x => PublicationLink(x.nome));
    }

    private static IEnumerable<DateTime> Last15Days()
    {
        return Enumerable.Range(0, 15).Select(x => DateTime.Today.AddDays(-x));
    }

    private static string PublicationsByDate(DateTime date)
    {
        return $"https://www.franca.sp.gov.br/pmf-diario/rest/diario/buscaPorArquivo/{date:dd-MM-yyyy}";
    }

    private static string PublicationLink(string filename)
    {
        return $"https://www.franca.sp.gov.br/arquivos/diario-oficial/documentos/{filename}";
    }

    private record Entry(string nome);
}