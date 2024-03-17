using Microsoft.Extensions.DependencyInjection;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers;

public static class Module
{
    public static IServiceCollection AddScrapers(this IServiceCollection services)
    {
        return services
            .AddHttpClient()
            .AddTransient<IPublicationProvider, Franca.PublicationProvider>()
            .AddTransient<IPublicationProvider, SaoCarlos.PublicationProvider>()
            .AddTransient<IPublicationProvider, Sorocaba.PublicationProvider>();
    }
}