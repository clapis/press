using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Polly.Registry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Providers.SP.SaoCarlos;

public class PublicationScraper(
    HttpClient httpClient,
    IPdfContentExtractor extractor,
    ResiliencePipelineProvider<string> polly
    ) : IPublicationScraper
{
    public string SourceId => "dom_sp_sao_carlos";
    
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
                    Date = GetDateFrom(link)
                };
            }
        }
    }

    private static IEnumerable<string> Pages()
    {
        // current & previous month
        yield return Page(DateTime.Today.AddMonths(-1));
        yield return Page(DateTime.Today);
    }

    private static string Page(DateTime date)
        => $"http://www.saocarlos.sp.gov.br/index.php/diario-oficial-{date.Year}/diario-oficial-{date.GetMonthNamePT_BR()}-{date.Year}.html";

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
            .QuerySelectorAll("a")
            .OfType<IHtmlAnchorElement>()
            .Select(x => x.Href)
            .Where(x => x.StartsWith("http://www.saocarlos.sp.gov.br/images/stories/diario_oficial"))
            .Where(x => x.EndsWith(".pdf"))
            .Distinct()
            .ToList();

        return links;
    }

    private static DateTime GetDateFrom(string url)
    {
        var match = Regex.Match(url, @"\d{2}-\d{2}-\d{4}");

        if (match.Success && DateTime.TryParseExact(match.Value, "dd-MM-yyyy", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result))
            return result;

        return DateTime.UtcNow;
    }
}