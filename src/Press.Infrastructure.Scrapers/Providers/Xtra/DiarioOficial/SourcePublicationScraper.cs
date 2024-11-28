using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Providers.Xtra.DiarioOficial;

public class PublicationScraper(
    HttpClient client, 
    IPdfContentExtractor extractor) : IPublicationScraper
{
    public string SourceId => "x_do_concursos";
    
    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var links = await GetAnnouncementsLinksAsync(cancellationToken);

        foreach (var link in links.Except(existing))
        {
            var contents = await extractor.ExtractAsync(link, cancellationToken);

            yield return new Publication
            {
                Url = link,
                SourceId = SourceId,
                Contents = contents,
                Date = DateTime.UtcNow.Date
            };
        }
    }

    private async Task<IEnumerable<string>> GetAnnouncementsLinksAsync(CancellationToken cancellationToken)
    {
        var data = await GetPublicTendersAsync(1, cancellationToken);
        
        return data.SelectMany(tender => tender.Announcements)
            .Select(announcement => announcement.Url)
            .Where(url => url.EndsWith(".pdf"))
            .Select(url => $"http://{url}");
    }
    
    private async Task<IEnumerable<PublicTender>> GetPublicTendersAsync(int page, CancellationToken cancellationToken)
    {
        var url = $"https://apidiario.wfonline.com.br/api/concursos?page={page}";

        var data = await client.GetFromJsonAsync<Data>(url, cancellationToken: cancellationToken);
                     
        if (data == null) throw new Exception("Content expected");

        if (data.CurrentPage == data.LastPage)
            return data.Tenders;

        var next = await GetPublicTendersAsync(data.CurrentPage + 1, cancellationToken);

        return data.Tenders.Union(next);
    }

    record Data([property: JsonPropertyName("data")] PublicTender[] Tenders, [property: JsonPropertyName("current_page")] int CurrentPage, [property: JsonPropertyName("last_page")] int LastPage);
    record PublicTender([property: JsonPropertyName("editais")] Announcement[] Announcements);
    record Announcement(string Url);
}