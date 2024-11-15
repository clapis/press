using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Providers.SaoCarlos;

public class SourcePublicationProvider(
    HttpClient httpClient,
    ResiliencePipelineProvider<string> polly,
    ILogger<SourcePublicationProvider> logger
    ) : ISourcePublicationProvider
{
    public string SourceId => "dom_sp_sao_carlos";

    public async Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken)
    {
        var publications = new List<Publication>();
        
        foreach (var page in Pages())
        {
            var links = await ScrapePageLinksAsync(Page(DateTime.Today), cancellationToken);

            publications.AddRange(links
                .Select(link => new Publication
                {
                    Url = link,
                    SourceId = SourceId,
                    Date = GetDateFrom(link)
                }));
        }

        return publications;
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