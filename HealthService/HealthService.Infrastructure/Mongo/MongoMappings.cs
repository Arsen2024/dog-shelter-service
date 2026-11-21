using HealthService.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace HealthService.Infrastructure.Mongo;

/// <summary>
/// Відображення POCO ↔ BSON, узгоджене з JSON Schema у database/init.js:
/// camelCase, Guid як рядок-UUID, _id і reference-поля як ObjectId, enum як рядок, null-поля не записуються.
/// </summary>
public static class MongoMappings
{
    private static readonly object Lock = new();
    private static bool _registered;

    public static void Register()
    {
        lock (Lock)
        {
            if (_registered) return;
            _registered = true;
        }

        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreIfNullConvention(true),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register(
            "HealthServiceConventions", conventions,
            t => t.Namespace != null && t.Namespace.StartsWith("HealthService.Domain"));

        var guidAsString = new GuidSerializer(BsonType.String);

        BsonClassMap.RegisterClassMap<MedicalRecord>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.MapMember(x => x.DogId).SetSerializer(guidAsString);
        });

        BsonClassMap.RegisterClassMap<Vaccination>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.MapMember(x => x.DogId).SetSerializer(guidAsString);
            cm.MapMember(x => x.MedicalRecordId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<VeterinaryVisit>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.MapMember(x => x.DogId).SetSerializer(guidAsString);
            cm.MapMember(x => x.MedicalRecordId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            cm.MapMember(x => x.RelatedVisitId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        // у документі поле називається "id", а не "vetId"
        BsonClassMap.RegisterClassMap<Veterinarian>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(x => x.VetId).SetElementName("id");
        });
    }
}
