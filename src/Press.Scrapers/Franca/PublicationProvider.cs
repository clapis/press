using System.Text.Json;
using Press.Core.Publications;

namespace Press.Scrapers.Franca
{
    public class PublicationProvider : IPublicationProvider
    {
        private readonly HttpClient _httpClient;

        public PublicationProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async IAsyncEnumerable<Publication> ProvideAsync(CancellationToken cancellationToken)
        {
            foreach (var date in Last15Days())
            {
                var links = await GetPublicationsByDateAsync(date, cancellationToken);
                
                foreach (var link in links)
                {
                    yield return new Publication
                    {
                        Url = link,
                        Date = date
                    };
                }
            }
        }

        private async Task<IEnumerable<string>> GetPublicationsByDateAsync(DateTime date, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(PublicationsByDate(date), cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(content)) return Enumerable.Empty<string>();

            return JsonSerializer.Deserialize<List<Entry>>(content)!
                .Select(x => PublicationLink(x.nome));
        }

        private static IEnumerable<DateTime> Last15Days() => 
            Enumerable.Range(0, 15).Select(x => DateTime.Today.AddDays(-x));

        private static string PublicationsByDate(DateTime date) => 
            $"https://www.franca.sp.gov.br/pmf-diario/rest/diario/buscaPorArquivo/{date:dd-MM-yyyy}";

        private static string PublicationLink(string filename) =>
            $"https://www.franca.sp.gov.br/arquivos/diario-oficial/documentos/{filename}";

        private record Entry(string nome);
    }
}