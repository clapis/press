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
                    Date = GetDateFromLink(link),
                    Source = PublicationSource.Sorocaba
                };
        }
    }

    private static IEnumerable<string> Pages()
    {
        return Enumerable.Range(1, 5).Select(Page);
    }

    private static string Page(int page)
    {
        return $"https://noticias.sorocaba.sp.gov.br/jornal-do-municipio/page/{page}/";
    }

    private Task<List<string>> ScrapePageLinksAsync(string url, CancellationToken cancellationToken)
    {
        return Policy
            .HandleResult<List<string>>(links => links.Count == 0)
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2 * i),
                (result, span) => logger.LogInformation("Retrying.."))
            .ExecuteAsync(async () => await ScrapePageLinksCoreAsync(url, cancellationToken));
    }

    private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url, cancellationToken);

        var links = document
            .QuerySelectorAll(".content-pdf a")
            .OfType<IHtmlAnchorElement>()
            .Select(x => x.Href)
            .Where(x => x.EndsWith(".pdf"))
            .Distinct()
            .ToList();

        logger.LogInformation("Found {LinksCount} links in page {Page}", links.Count, url);

        return links;
    }

    private DateTime GetDateFromLink(string link)
    {
        try
        {
            var pattern =
                @"\/wp-content\/uploads\/(?<year>\d+)\/(?<month_number>\d+)\/(?<number>\d+)-(?<day>\d+)-DE-(?<month>\w+)-.+\.pdf$";

            var match = Regex.Match(link, pattern);

            var year = int.Parse(match.Groups["year"].Value);
            // var month = int.Parse(match.Groups["month_digit"].Value);
            var month = ParseMonth(match.Groups["month"].Value);
            var day = int.Parse(match.Groups["day"].Value);

            return new DateTime(year, month, day);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Failed to infer date from link {link}", ex);

            return DateTime.UtcNow.Date;
        }
    }

    private static int ParseMonth(string month)
    {
        return month.ToLower() switch
        {
            "janeiro" => 01,
            "fevereiro" => 02,
            "marco" => 03,
            "abril" => 04,
            "maio" => 05,
            "junho" => 06,
            "julho" => 07,
            "agosto" => 08,
            "setembro" => 09,
            "outubro" => 10,
            "novembro" => 11,
            "dezembro" => 12,
            _ => throw new ArgumentOutOfRangeException($"Cannot parse '{month}'")
        };
    }
}