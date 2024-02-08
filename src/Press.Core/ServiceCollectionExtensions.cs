using Microsoft.Extensions.DependencyInjection;
using Press.Core.Alerts;
using Press.Core.Publications;
using Press.Core.Publications.Extractors;

namespace Press.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPressCore(this IServiceCollection services)
        {
            services
                .AddTransient<AlertService>()
                .AddTransient<ReportService>()
                .AddTransient<SynchronizationService>();

            services
                .AddTransient<IContentExtractor, PigExtractor>();
                // .AddTransient<IContentExtractor, SharpTextExtractor>();

            return services;
        }
    }
}