using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.SaoCarlos;

public class SourcePublicationProvider(
    ILogger<SourcePublicationProvider> logger,
    ResiliencePipelineProvider<string> polly
    ) : ISourcePublicationProvider
{
    public string SourceId => "dom_sp_sao_carlos";

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
        // current & previous month
        yield return Page(DateTime.Today);
        yield return Page(DateTime.Today.AddMonths(-1));
    }

    private static string Page(DateTime date)
        => $"http://www.saocarlos.sp.gov.br/index.php/diario-oficial-{date.Year}/diario-oficial-{MapMonth(date)}-{date.Year}.html";

    private async Task<List<string>> ScrapePageLinksAsync(string url, CancellationToken cancellationToken)
    {
        var policy = polly.GetPipeline<List<string>>("no-links");

        return await policy.ExecuteAsync(async token 
            => await ScrapePageLinksCoreAsync(url, token), cancellationToken);
    }

    private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url, cancellationToken);

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

    private static string MapMonth(DateTime date)
    {
        return date.Month switch
        {
            01 => "janeiro",
            02 => "fevereiro",
            03 => "marco",
            04 => "abril",
            05 => "maio",
            06 => "junho",
            07 => "julho",
            08 => "agosto",
            09 => "setembro",
            10 => "outubro",
            11 => "novembro",
            12 => "dezembro",
            _ => throw new ArgumentOutOfRangeException($"month '{date.Month}'")
        };
    }
}