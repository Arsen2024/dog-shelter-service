using HealthService.Domain.Common;
using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace HealthService.Infrastructure.Mongo;

/// <summary>
/// BSON-мапінг налаштовується один раз під час старту — у домені BSON-атрибутів немає.
/// Сутності: BsonClassMap з явними назвами елементів (camelCase) і приватними полями колекцій (MapField).
/// Незмінні Value Objects: серіалізатори створюють їх через конструктор (див. ValueObjectSerializers.cs).
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
            type => type.Namespace is not null && type.Namespace.StartsWith("HealthService.Domain"));

        RegisterValueObjectSerializers();
        RegisterEntities();
    }

    private static void RegisterValueObjectSerializers()
    {
        BsonSerializer.RegisterSerializer(new VeterinarianSerializer());
        BsonSerializer.RegisterSerializer(new MoneySerializer());
        BsonSerializer.RegisterSerializer(new DogSnapshotSerializer());
        BsonSerializer.RegisterSerializer(new AnesthesiaSerializer());
        BsonSerializer.RegisterSerializer(new AllergySerializer());
        BsonSerializer.RegisterSerializer(new ChronicConditionSerializer());
        BsonSerializer.RegisterSerializer(new MedicalNoteSerializer());
        BsonSerializer.RegisterSerializer(new VaccineEntrySerializer());
    }

    private static void RegisterEntities()
    {
        var guidAsString = new GuidSerializer(BsonType.String);

        // спільні поля BaseEntity
        BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
        {
            cm.SetIgnoreExtraElements(true);
            cm.MapIdMember(x => x.Id).SetElementName("_id");
            cm.MapMember(x => x.CreatedAt).SetElementName("createdAt");
            cm.MapMember(x => x.UpdatedAt).SetElementName("updatedAt").SetIgnoreIfNull(true);
            cm.MapMember(x => x.Version).SetElementName("version");
        });

        BsonClassMap.RegisterClassMap<MedicalRecord>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(x => x.DogId).SetElementName("dogId").SetSerializer(guidAsString);
            cm.MapMember(x => x.DogName).SetElementName("dogName");
            cm.MapMember(x => x.BloodType).SetElementName("bloodType").SetIgnoreIfNull(true);
            cm.MapField("_allergies").SetElementName("allergies");
            cm.MapField("_chronicConditions").SetElementName("chronicConditions");
            cm.MapField("_notes").SetElementName("notes");
        });

        BsonClassMap.RegisterClassMap<Vaccination>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(x => x.DogId).SetElementName("dogId").SetSerializer(guidAsString);
            cm.MapMember(x => x.DogName).SetElementName("dogName");
            cm.MapMember(x => x.MedicalRecordId).SetElementName("medicalRecordId");
            cm.MapMember(x => x.VaccinatedAt).SetElementName("vaccinatedAt");
            cm.MapMember(x => x.NextDueAt).SetElementName("nextDueAt").SetIgnoreIfNull(true);
            cm.MapMember(x => x.Veterinarian).SetElementName("veterinarian").SetIgnoreIfNull(true);
            cm.MapField("_vaccines").SetElementName("vaccines");
        });

        BsonClassMap.RegisterClassMap<VeterinaryVisit>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(x => x.DogId).SetElementName("dogId").SetSerializer(guidAsString);
            cm.MapMember(x => x.DogName).SetElementName("dogName");
            cm.MapMember(x => x.MedicalRecordId).SetElementName("medicalRecordId");
            cm.MapMember(x => x.RelatedVisitId).SetElementName("relatedVisitId").SetIgnoreIfNull(true);
            cm.MapMember(x => x.DogSnapshot).SetElementName("dogSnapshot").SetIgnoreIfNull(true);
            cm.MapMember(x => x.VisitDate).SetElementName("visitDate");
            cm.MapMember(x => x.VisitType).SetElementName("visitType")
                .SetSerializer(new EnumSerializer<VisitType>(BsonType.String));
            cm.MapMember(x => x.Veterinarian).SetElementName("veterinarian").SetIgnoreIfNull(true);
            cm.MapMember(x => x.Diagnosis).SetElementName("diagnosis").SetIgnoreIfNull(true);
            cm.MapMember(x => x.WeightKg).SetElementName("weightKg").SetIgnoreIfNull(true);
            cm.MapMember(x => x.TemperatureC).SetElementName("temperatureC").SetIgnoreIfNull(true);
            cm.MapMember(x => x.Anesthesia).SetElementName("anesthesia").SetIgnoreIfNull(true);
            cm.MapMember(x => x.OperationNotes).SetElementName("operationNotes").SetIgnoreIfNull(true);
            cm.MapMember(x => x.TriageLevel).SetElementName("triageLevel").SetIgnoreIfNull(true);
            cm.MapMember(x => x.Cost).SetElementName("cost").SetIgnoreIfNull(true);
            cm.MapField("_symptoms").SetElementName("symptoms");
            cm.MapField("_recommendations").SetElementName("recommendations");
        });
    }
}
