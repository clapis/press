using Microsoft.Extensions.DependencyInjection;
using Press.Core.Alerts;
using Press.Core.Publications;

namespace Press.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPressCore(this IServiceCollection services)
        {
            services
                .AddHttpClient()
                .AddTransient<AlertService>()
                .AddTransient<ReportService>()
                .AddTransient<SynchronizationService>();

            return services;
        }
    }
}