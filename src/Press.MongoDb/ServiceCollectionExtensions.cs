using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Press.Core.Alerts;
using Press.Core.Publications;
using Press.MongoDb.Configuration;
using Press.MongoDb.Stores;

namespace Press.MongoDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoPress(this IServiceCollection services, MongoDbOptions options)
        {
            BsonClassMappings.Configure();

            services
                .AddDefaultMongoClient(options.ConnectionString)
                .AddMongoDatabase("press")
                .AddMongoCollection<Publication>("publications")
                .AddSingleton<IPublicationStore, PublicationStore>()
                .AddMongoCollection<Alert>("alerts")
                .AddSingleton<IAlertStore, AlertStore>()
                .AddHostedService<Indexes>();

            return services;
        }

        private static IServiceCollection AddDefaultMongoClient(this IServiceCollection services, string connectionString)
            => services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        private static IServiceCollection AddMongoDatabase(this IServiceCollection services, string name)
            => services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(name));

        private static IServiceCollection AddMongoCollection<T>(this IServiceCollection services, string name)
            => services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<T>(name));


        private static IServiceCollection AddMongoClient(this IServiceCollection services, string connectionString)
        {
            return services.AddSingleton<IMongoClient>(_ =>
            {
                var settings = MongoClientSettings.FromConnectionString(connectionString);

                settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
                settings.ReadPreference = ReadPreference.SecondaryPreferred;
                settings.WriteConcern = WriteConcern.Acknowledged;
                settings.ReadConcern = ReadConcern.Local;
                settings.RetryWrites = true;
                settings.RetryReads = true;

                return new MongoClient(settings);
            });
        }
    }
}