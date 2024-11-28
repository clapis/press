using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Press.Core.Infrastructure.Scrapers;
using Press.Infrastructure.Scrapers.Extractors;

namespace Press.Infrastructure.Scrapers;

public static class Module
{
    public static IServiceCollection AddScrapers(this IServiceCollection services, ScrapersSettings settings)
    {
        services
            .AddPollyPipelines()
            .AddPublicationProviders()
            .AddScrapersHttpClient(settings);
        
        services.AddTransient<IPdfContentExtractor, PdfContentExtractor>();

        return services;
    }

    private static IServiceCollection AddScrapersHttpClient(this IServiceCollection services, ScrapersSettings settings)
    {
        return services
            .ConfigureHttpClientDefaults(builder => builder
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(
                        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseProxy = settings.UseProxy,
                    Proxy = settings.UseProxy ? new WebProxy(settings.ProxyAddress) : null,
                    AllowAutoRedirect = false
                }));
    }

    private static IServiceCollection AddPublicationProviders(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .Where(x => x.IsAssignableTo(typeof(IPublicationScraper)))
            .ToList()
            .ForEach(provider => services.AddTransient(typeof(IPublicationScraper), provider));

        return services;
    }

    private static IServiceCollection AddPollyPipelines(this IServiceCollection services)
    {
        services.AddResiliencePipeline<string, List<string>>("no-links", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions<List<string>>()
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<List<string>>()
                    .HandleResult(x => x.Count == 0)
            });
        });

        return services;
    }
}