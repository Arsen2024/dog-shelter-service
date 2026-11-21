namespace DogsService.Application.Queries.Dog;

/// <summary>Whitelist полів, за якими дозволено сортування (захист від довільних виразів у Dynamic LINQ).</summary>
public static class DogSortFields
{
    public static readonly IReadOnlyDictionary<string, string> Map =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Name",
            ["color"] = "Color",
            ["tailLength"] = "TailLength",
            ["weight"] = "Weight",
            ["status"] = "Status",
            ["createdAt"] = "CreatedAt"
        };
}
