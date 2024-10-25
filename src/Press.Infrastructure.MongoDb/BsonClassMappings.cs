using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Press.Core.Domain;

namespace Press.Infrastructure.MongoDb;

internal static class BsonClassMappings
{
    public static void Configure()
    {
        BsonClassMap.RegisterClassMap<Source>(map =>
        {
            map.AutoMap();
            map.MapIdProperty(x => x.Id);
        });
        
        BsonClassMap.RegisterClassMap<Publication>(map =>
        {
            map.AutoMap();
            map.MapIdProperty(x => x.Id)
                .SetIdGenerator(new StringObjectIdGenerator())
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<Alert>(map =>
        {
            map.AutoMap();
            map.MapIdProperty(x => x.Id)
                .SetIdGenerator(new StringObjectIdGenerator())
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
    }
}