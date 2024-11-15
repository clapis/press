using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Providers.Xtra.DiarioOficial;

public class SourcePublicationProvider(HttpClient client) 
    : ISourcePublicationProvider
{
    public string SourceId => "x_do_concursos";
    
    public async Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken)
    {
        var links = await GetAnnouncementsLinksAsync(cancellationToken);
        
        return links.Select(link => new Publication
            {
                Url = link, 
                SourceId = SourceId, 
                Date = DateTime.UtcNow.Date
            })
            .ToList();
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