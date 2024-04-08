using System.Net;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Sorocaba;

public class PublicationProvider(ILogger<PublicationProvider> logger) : IPublicationProvider
{
    public bool IsEnabled => true;

    public PublicationSource Source => PublicationSource.Sorocaba;

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
                    Source = Source,
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
        var policy = new ResiliencePipelineBuilder<List<string>>()
            .AddRetry(new RetryStrategyOptions<List<string>>()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<List<string>>()
                    .HandleResult(x => x.Count == 0),
                OnRetry = _ =>
                {
                    logger.LogWarning("No links founds at {Url}, retrying..", url);

                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        return await policy.ExecuteAsync(async token 
            => await ScrapePageLinksCoreAsync(url, token), cancellationToken);
    }

    private async Task<List<string>> ScrapePageLinksCoreAsync(string url, CancellationToken cancellationToken)
    {
        var config = Configuration.Default
            .With<IRequester>(_ => new DefaultHttpRequester("Mozilla/5.0", request =>
            {
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 3;
            }))
            .WithDefaultLoader();

        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(url, cancellationToken);

        if (document.StatusCode != HttpStatusCode.OK)
            logger.LogWarning("Unexpected status code: {StatusCode}", document.StatusCode);

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