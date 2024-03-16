using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Sorocaba;

public class PublicationProvider(ILogger<PublicationProvider> logger) : IPublicationProvider
{
    public async IAsyncEnumerable<Publication> ProvideAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var page in Pages())
        {
            var links = await ScrapePageLinksAsync(page, cancellationToken);

            foreach (var link in links)
                yield return new Publication
                {
                    Url = link,
                    Date = DateTime.UtcNow.Date,
                    Source = PublicationSource.Sorocaba
                };
        }
    }

    private static IEnumerable<string> Pages() 
        => Enumerable.Range(1, 3).Select(Page);

    private static string Page(int page) 
        => $"https://noticias.sorocaba.sp.gov.br/jornal/page/{page}/";

    private Task<List<string>> ScrapePageLinksAsync(string url, CancellationToken cancellationToken)
    {
        return Policy
            .HandleResult<List<string>>(links => links.Count == 0)
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2 * i),
                (_, _) => logger.LogWarning("No links founds at {Url}, retrying..", url))
            .ExecuteAsync(async () => await ScrapePageLinksCoreAsync(url, cancellationToken));
    }

    private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url, cancellationToken);

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