using HealthService.Domain.Enums;
using HealthService.Domain.ValueObjects;
using HealthService.Infrastructure.Queries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HealthService.Infrastructure.Mongo;

// Незмінні Value Objects вбудовуються в документи як вкладені об'єкти й створюються ЧЕРЕЗ КОНСТРУКТОР
// (він перевіряє інваріанти). MapCreator вимагає, щоб усі аргументи конструктора були в документі,
// а в документах Лаб. 01 необов'язкових полів (email, weightKg, notedAt, ...) немає — тому читання зроблено
// серіалізаторами: документ читається як BsonDocument, відсутні поля стають null, далі викликається конструктор.

internal static class BsonWriteExtensions
{
    public static BsonDocument Put(this BsonDocument d, string name, string? value)
    {
        if (value is not null) d[name] = value;
        return d;
    }

    public static BsonDocument Put(this BsonDocument d, string name, double? value)
    {
        if (value.HasValue) d[name] = value.Value;
        return d;
    }

    public static BsonDocument Put(this BsonDocument d, string name, int? value)
    {
        if (value.HasValue) d[name] = value.Value;
        return d;
    }

    public static BsonDocument Put(this BsonDocument d, string name, DateTime? value)
    {
        if (value.HasValue) d[name] = new BsonDateTime(value.Value);
        return d;
    }
}

internal abstract class ValueObjectSerializer<T> : SerializerBase<T> where T : class
{
    protected abstract T FromDocument(BsonDocument document);

    protected abstract BsonDocument ToDocument(T value);

    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null!;
        }

        return FromDocument(BsonDocumentSerializer.Instance.Deserialize(context));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
            return;
        }

        BsonDocumentSerializer.Instance.Serialize(context, ToDocument(value));
    }
}

internal sealed class VeterinarianSerializer : ValueObjectSerializer<Veterinarian>
{
    protected override Veterinarian FromDocument(BsonDocument d)
    {
        var email = d.GetStringOrNull("email");

        return new Veterinarian(
            d.GetStringOrNull("id"),
            d.GetStringOrNull("name") ?? string.Empty,
            string.IsNullOrWhiteSpace(email) ? null : new Email(email));
    }

    protected override BsonDocument ToDocument(Veterinarian v) =>
        new BsonDocument().Put("id", v.VetId).Put("name", v.Name).Put("email", v.Email?.Value);
}

internal sealed class MoneySerializer : ValueObjectSerializer<Money>
{
    protected override Money FromDocument(BsonDocument d) =>
        new(d.GetDecimalOrNull("amount") ?? 0, d.GetStringOrNull("currency") ?? string.Empty);

    protected override BsonDocument ToDocument(Money v) =>
        new() { { "amount", new BsonDecimal128(v.Amount) }, { "currency", v.Currency } };
}

internal sealed class DogSnapshotSerializer : ValueObjectSerializer<DogSnapshot>
{
    protected override DogSnapshot FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("name") ?? string.Empty, d.GetStringOrNull("breed"), d.GetDoubleOrNull("weightKg"));

    protected override BsonDocument ToDocument(DogSnapshot v) =>
        new BsonDocument().Put("name", v.Name).Put("breed", v.Breed).Put("weightKg", v.WeightKg);
}

internal sealed class AnesthesiaSerializer : ValueObjectSerializer<Anesthesia>
{
    protected override Anesthesia FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("type") ?? string.Empty, d.GetIntOrNull("durationMin"));

    protected override BsonDocument ToDocument(Anesthesia v) =>
        new BsonDocument().Put("type", v.Type).Put("durationMin", v.DurationMin);
}

internal sealed class AllergySerializer : ValueObjectSerializer<Allergy>
{
    protected override Allergy FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("substance") ?? string.Empty,
            Enum.Parse<AllergySeverity>(d.GetStringOrNull("severity") ?? nameof(AllergySeverity.Mild)),
            d.GetDateOrNull("notedAt"));

    protected override BsonDocument ToDocument(Allergy v) =>
        new BsonDocument().Put("substance", v.Substance).Put("severity", v.Severity.ToString()).Put("notedAt", v.NotedAt);
}

internal sealed class ChronicConditionSerializer : ValueObjectSerializer<ChronicCondition>
{
    protected override ChronicCondition FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("name") ?? string.Empty, d.GetDateOrNull("diagnosedAt"), d.GetStringOrNull("notes"));

    protected override BsonDocument ToDocument(ChronicCondition v) =>
        new BsonDocument().Put("name", v.Name).Put("diagnosedAt", v.DiagnosedAt).Put("notes", v.Notes);
}

internal sealed class MedicalNoteSerializer : ValueObjectSerializer<MedicalNote>
{
    protected override MedicalNote FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("text") ?? string.Empty, d.GetStringOrNull("author"), d.GetDateOrNull("createdAt") ?? DateTime.UtcNow);

    protected override BsonDocument ToDocument(MedicalNote v) =>
        new BsonDocument().Put("text", v.Text).Put("author", v.Author).Put("createdAt", v.CreatedAt);
}

internal sealed class VaccineEntrySerializer : ValueObjectSerializer<VaccineEntry>
{
    protected override VaccineEntry FromDocument(BsonDocument d) =>
        new(d.GetStringOrNull("name") ?? string.Empty, d.GetStringOrNull("manufacturer"),
            d.GetStringOrNull("batchNumber"), d.GetDoubleOrNull("doseMl"));

    protected override BsonDocument ToDocument(VaccineEntry v) =>
        new BsonDocument().Put("name", v.Name).Put("manufacturer", v.Manufacturer)
            .Put("batchNumber", v.BatchNumber).Put("doseMl", v.DoseMl);
}
