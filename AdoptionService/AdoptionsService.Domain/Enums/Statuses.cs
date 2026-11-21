namespace AdoptionsService.Domain.Enums;

/// <summary>Статуси заявки (зберігаються в БД рядками, як OrderStatus в eShop).</summary>
public static class ApplicationStatuses
{
    public const string Submitted = "Submitted";
    public const string UnderReview = "UnderReview";
    public const string Approved = "Approved";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Submitted, UnderReview, Approved, Completed, Rejected, Cancelled];
}

/// <summary>Статуси собаки (копія з DogsService, власник — Каталог).</summary>
public static class DogStatuses
{
    public const string Available = "Available";
    public const string Reserved = "Reserved";
    public const string Adopted = "Adopted";
    public const string UnderTreatment = "UnderTreatment";

    public static readonly string[] All = [Available, Reserved, Adopted, UnderTreatment];
}
