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
        return services
            .AddHttpClient()
            .AddPollyPipelines()
            .AddPublicationProviders();
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
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<List<string>>()
                    .HandleResult(x => x.Count == 0)
            });
        });

        return services;
    }
}