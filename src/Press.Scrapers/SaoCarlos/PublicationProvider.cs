using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Polly;
using Press.Core.Publications;

namespace Press.Scrapers.SaoCarlos
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
                        Source = PublicationSource.SaoCarlos
                    };
                }
            }
        }
        
        private static IEnumerable<string> Pages()
        {
            // current & previous month
            yield return Page(DateTime.Today);
            yield return Page(DateTime.Today.AddMonths(-1));
        }
        
        private static string Page(DateTime date) => 
            $"http://www.saocarlos.sp.gov.br/index.php/diario-oficial-{date.Year}/diario-oficial-{MapMonth(date)}-{date.Year}.html";

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
                .QuerySelectorAll("a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .Where(x => x.StartsWith("http://www.saocarlos.sp.gov.br/images/stories/diario_oficial"))
                .Where(x => x.EndsWith(".pdf"))
                .Distinct()
                .ToList();
            
            _logger.LogInformation("Found {LinksCount} links in page {Page}", links.Count, url);
            
            return links;
        }

        private DateTime GetDateFromLink(string link)
        {
            var pattern = @"\/diario_oficial_\d{4}\/DO_(?<date>\d{8})_.+\.pdf$";

            var match = Regex.Match(link, pattern);

            return DateTime.ParseExact(match.Groups["date"].Value, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
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
}