using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Press.Core.Infrastructure.Cache;

namespace Press.Core;

public static class Module
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        
        services.AddMediatR(x 
            => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddTransient<ICachedSourceStore, CachedSourceStore>();

        return services;
    }
}