using Microsoft.Extensions.DependencyInjection;
using Press.Core.Infrastructure;
using Press.Infrastructure.Postmark.Services;

namespace Press.Infrastructure.Postmark;

public static class Module
{
    public static IServiceCollection AddPostmark(this IServiceCollection services, PostmarkSettings settings)
    {
        services.AddSingleton(settings);

        services.AddTransient<INotificationService, NotificationService>();

        return services;
    }
}