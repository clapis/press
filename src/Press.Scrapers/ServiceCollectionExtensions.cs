using Microsoft.Extensions.DependencyInjection;
using Press.Core.Publications;

namespace Press.Scrapers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScrapers(this IServiceCollection services)
        {
            return services
                .AddTransient<IContentExtractor, ContentExtractor>()
                .AddTransient<IPublicationScraper, PublicationScraper>();
        }
    }
}