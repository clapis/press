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
            .AddTransient<IPublicationProvider, Franca.PublicationProvider>()
            .AddTransient<IPublicationProvider, SaoCarlos.PublicationProvider>()
            .AddTransient<IPublicationProvider, Sorocaba.PublicationProvider>();
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