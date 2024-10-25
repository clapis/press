using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Press.Core.Features.Sources.Scrape;

namespace Press.Core;

public static class Module
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddMediatR(x 
            => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services
            .AddTransient<ScrappingService>()
            .AddTransient<IPdfContentExtractor, PigExtractor>()
            .AddTransient<IPublicationProvider, PublicationProvider>();

        return services;
    }
}