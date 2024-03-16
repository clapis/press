using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;
using Press.Infrastructure.MongoDb.Configuration;
using Press.Infrastructure.MongoDb.Stores;

namespace Press.Infrastructure.MongoDb;

public static class Module
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, MongoDbSettings settings)
    {
        BsonClassMappings.Configure();

        services
            .AddMongoClient(settings.ConnectionString)
            .AddMongoDatabase("press");

        services
            .AddMongoCollection<Alert>("alerts")
            .AddSingleton<IAlertStore, AlertStore>();

        services
            .AddMongoCollection<Publication>("publications")
            .AddSingleton<IPublicationStore, PublicationStore>();
        
        services
            .AddHostedService<Indexes>();

        return services;
    }
    
    private static IServiceCollection AddMongoDatabase(this IServiceCollection services, string name) 
        => services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(name));

    private static IServiceCollection AddMongoCollection<T>(this IServiceCollection services, string name) 
        => services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<T>(name));

    private static IServiceCollection AddMongoClient(this IServiceCollection services, string connectionString)
    {
        return services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            var subscriber = new DiagnosticsActivityEventSubscriber(new()
            {
                CaptureCommandText = true
            });

            settings.ClusterConfigurator = cb => cb.Subscribe(subscriber);

            return new MongoClient(settings);
        });
    }
}