using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Html.Dom;
using Polly.Registry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Providers.SP.Sorocaba;

public class PublicationScraper(
    HttpClient httpClient,
    IPdfContentExtractor extractor,
    ResiliencePipelineProvider<string> polly) 
    : IPublicationScraper
{
    public string SourceId => "dom_sp_sorocaba";

    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var page in Pages())
        {
            var links = await ScrapePageLinksAsync(page, cancellationToken);

            foreach (var link in links.Except(existing))
            {
                using var file = await httpClient.DownloadAsync(link, cancellationToken);
                
                var contents = await extractor.ExtractTextAsync(file.Path, cancellationToken);

                yield return new Publication
                {
                    Url = link,
                    SourceId = SourceId,
                    Contents = contents,
                    Date = DateTime.UtcNow.Date
                };
            }
        }
    }

    private static IEnumerable<string> Pages()
    {
        yield return "https://noticias.sorocaba.sp.gov.br/jornal/page/2/";
        yield return "https://noticias.sorocaba.sp.gov.br/jornal/";
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
            .Select(x => x.Href.Replace("http://", "https://"))
            .Where(x => x.EndsWith(".pdf"))
            .Distinct()
            .ToList();

        return links;
    }
}