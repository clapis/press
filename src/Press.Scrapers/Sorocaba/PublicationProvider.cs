using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly;
using Press.Core.Publications;

namespace Press.Scrapers.Sorocaba
{
    public class PublicationProvider : IPublicationProvider
    {
        private readonly ILogger<PublicationProvider> _logger;
        public PublicationProvider(ILogger<PublicationProvider> logger) => _logger = logger;

        public async IAsyncEnumerable<Publication> ProvideAsync(CancellationToken cancellationToken)
        {
            foreach (var page in Pages())
            {
                var links = await ScrapePageLinksAsync(page, cancellationToken);
                
                foreach (var link in links)
                {
                    yield return new Publication
                    {
                        Url = link,
                        Date = GetDateFromLink(link),
                        Source = PublicationSource.Sorocaba
                    };
                }
            }
        }
        
        private static IEnumerable<string> Pages() => 
            Enumerable.Range(1, 3).Select(Page);

        private static string Page(int page) => 
            $"https://noticias.sorocaba.sp.gov.br/jornal/page/{page}/";

        private Task<List<string>> ScrapePageLinksAsync(string url, CancellationToken cancellationToken)
        {
            return Policy
                .HandleResult<List<string>>(links => links.Count == 0)
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2 * i), (result, span) => _logger.LogInformation("Retrying.."))
                .ExecuteAsync(async () => await ScrapePageLinksCoreAsync(url, cancellationToken));
        }
        
        private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url, cancellationToken);
            
            var links = document
                .QuerySelectorAll("#jornal-home > a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .Where(x => x.EndsWith(".pdf"))
                .Distinct()
                .ToList();
            
            _logger.LogInformation("Found {LinksCount} links in page {Page}", links.Count, url);
            
            return links;
        }

        private DateTime GetDateFromLink(string link)
        {
            var pattern = @"^http:\/\/noticias\.sorocaba\.sp\.gov\.br\/wp-content\/uploads\/(?<year>\d+)\/(?<month>\d+)\/noticias\.sorocaba\.sp\.gov\.br-(?<number>\d+)-(?<day>\d+)-.+\.pdf$";

            var match = Regex.Match(link, pattern);
            
            var year = int.Parse(match.Groups["year"].Value);
            var month = int.Parse(match.Groups["month"].Value);
            var day = int.Parse(match.Groups["day"].Value);

            return new DateTime(year, month, day);
        }
    }
}