using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Providers.Sorocaba;

public class SourcePublicationProvider(
    HttpClient httpClient,
    ResiliencePipelineProvider<string> polly,
    ILogger<SourcePublicationProvider> logger) 
    : ISourcePublicationProvider
{
    public string SourceId => "dom_sp_sorocaba";

    public async Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken)
    {
        var publications = new List<Publication>();
        
        foreach (var page in Pages())
        {
            var links = await ScrapePageLinksAsync(page, cancellationToken);

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

    private static IEnumerable<string> Pages()
    {
        yield return "https://noticias.sorocaba.sp.gov.br/jornal/";
        yield return "https://noticias.sorocaba.sp.gov.br/jornal/page/2/";
    }

    private async Task<List<string>> ScrapePageLinksAsync(string url, CancellationToken cancellationToken)
    {
        var policy = polly.GetPipeline<List<string>>("no-links");

        return await policy.ExecuteAsync(async token 
            => await ScrapePageLinksCoreAsync(url, token), cancellationToken);
    }

    private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
    {
        var content = await httpClient.GetStringAsync(url, cancellationToken);

        var document = await BrowsingContext.New()
            .OpenAsync(req => req.Content(content), cancellationToken);

        var links = document
            .QuerySelectorAll("#jornal-home a")
            .OfType<IHtmlAnchorElement>()
            .Select(x => x.Href)
            .Where(x => x.EndsWith(".pdf"))
            .Distinct()
            .ToList();

        return links;
    }
}