using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Press.Core.Alerts;
using Press.Core.Publications;

namespace Press.MongoDb.Services
{
    public static class BsonClassMappings
    {
        public static void Configure()
        {
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
}