using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers;

public static class Module
{
    public static IServiceCollection AddScrapers(this IServiceCollection services)
    {
        services
            .ConfigureHttpClientDefaults(builder => builder
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(
                        "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new WebProxy("socks5://openvpn-dante-proxy.openvpn-dante-proxy:1080")
                }));
            
        services
            .AddPollyPipelines()
            .AddPublicationProviders();

        return services;
    }

    private static IServiceCollection AddPublicationProviders(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .Where(x => x.IsAssignableTo(typeof(ISourcePublicationProvider)))
            .ToList()
            .ForEach(provider => services.AddTransient(typeof(ISourcePublicationProvider), provider));

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