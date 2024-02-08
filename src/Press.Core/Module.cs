using Microsoft.Extensions.DependencyInjection;
using Press.Core.Features.Alerts.Alert;
using Press.Core.Features.Alerts.Report;
using Press.Core.Features.Sources.Scrape;
using Press.Core.Features.Sources.Scrape.Extractors;

namespace Press.Core;

public static class Module
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services
            .AddTransient<AlertHandler>()
            .AddTransient<ReportHandler>()
            .AddTransient<SourcesScrapeHandler>();

        services
            .AddTransient<IPdfContentExtractor, PigExtractor>();

        return services;
    }
}