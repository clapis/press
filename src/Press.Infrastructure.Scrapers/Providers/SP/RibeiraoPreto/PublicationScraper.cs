using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Providers.SP.RibeiraoPreto;

public class PublicationScraper(
    HttpClient client, 
    IPdfContentExtractor extractor) : IPublicationScraper 
{
    public string SourceId => "dom_sp_ribeirao_preto";
    
    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var dates = Enumerable.Range(0, 6)
            .Select(x => DateTime.UtcNow.Date.AddDays(-x))
            .Reverse();

        foreach (var date in dates)
        {
            var links = await ProvideLinksAsync(date, cancellationToken);

            foreach (var link in links.ExceptBy(existing, x => x.BrowserUrl))
            {
                var contents = await extractor.ExtractAsync(link.DownloadUrl, cancellationToken);

                yield return new Publication
                {
                    Date = date,
                    Contents = contents,
                    SourceId = SourceId,
                    Url = link.BrowserUrl
                };
            }
        }
    }
    
    private async Task<IEnumerable<Link>> ProvideLinksAsync(DateTime date, CancellationToken cancellationToken)
    {
        var url = $"https://cespro.com.br/_data/api.php?cdMunicipio=9314&dtDiario={date:yyyy-MM-dd}&operacao=content-diario-oficial-resumo";

        using var response = await client.PostAsync(url, JsonContent.Create(new { ID = 9314 }, 
            options: new JsonSerializerOptions { PropertyNamingPolicy = null }),  cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        
        if (!content.GetProperty("dados_diario_oficial").GetProperty("dados_diario_atual")
            .TryGetProperty("tx_url_file", out var value)) return [];

        var downloadUrl = value.GetString() + "raw=1";
        var browserUrl = $"https://cespro.com.br/visualizarDiarioOficialLeituraDigital.php?cdMunicipio=9314&dtDiario={date:yyyy-MM-dd}";
        
        return [ new Link(downloadUrl, browserUrl) ];
    }

    private record Link(string DownloadUrl, string BrowserUrl);
}