using MongoDB.Bson;

namespace HealthService.Infrastructure.Queries;

internal static class BsonReadExtensions
{
    public static string? GetStringOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.AsString : null;

    public static DateTime? GetDateOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToUniversalTime() : null;

    public static double? GetDoubleOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToDouble() : null;

    public static int? GetIntOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToInt32() : null;

    public static decimal? GetDecimalOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToDecimal() : null;

    public static long GetLong(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToInt64() : 0;

    public static ObjectId? GetObjectIdOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && v.IsObjectId ? v.AsObjectId : null;

    public static BsonDocument? GetDocOrNull(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && v.IsBsonDocument ? v.AsBsonDocument : null;

    public static List<string> GetStrings(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && v.IsBsonArray
            ? v.AsBsonArray.Where(x => x.IsString).Select(x => x.AsString).ToList()
            : [];

    public static IEnumerable<BsonDocument> GetDocs(this BsonDocument d, string name) =>
        d.TryGetValue(name, out var v) && v.IsBsonArray
            ? v.AsBsonArray.Where(x => x.IsBsonDocument).Select(x => x.AsBsonDocument)
            : [];
}
