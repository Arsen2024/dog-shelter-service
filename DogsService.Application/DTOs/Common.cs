using System.ComponentModel.DataAnnotations;

namespace DogsService.Application.DTOs;

/// <summary>Guid не може бути порожнім (00000000-...).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute() : base("The {0} field must be a non-empty GUID.") { }

    public override bool IsValid(object? value) => value is Guid g && g != Guid.Empty;
}

public static class Patterns
{
    public const string DogStatus = "^(?i)(Available|Reserved|Adopted|UnderTreatment)$";
    public const string Gender = "^(Male|Female|Unknown)$";
    public const string SortBy = "^(?i)(name|color|tailLength|weight|createdAt)$";
    public const string SortDir = "^(?i)(asc|desc)$";
}

/// <summary>Сторінка результатів: база повертає лише items сторінки, totalCount — кількість усіх записів за фільтром.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }

    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1
        };
    }
}
