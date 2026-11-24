using System.Linq.Expressions;
using Ardalis.Specification;
using DogsService.Domain.Entities;
using DogsService.Domain.Enums;

namespace DogsService.Infrastructure.Specifications;

/// <summary>
/// Фільтрація, сортування і пагінація списку собак. Where / OrderBy / Skip / Take будуються
/// до ToListAsync(), тож база повертає лише одну сторінку. Для totalCount репозиторій
/// застосовує цю ж специфікацію лише з критеріями (без Skip/Take).
/// </summary>
public class DogsFilterSpec : Specification<DogEntity>
{
    public static readonly string[] AllowedSortFields = ["name", "color", "tailLength", "weight", "createdAt"];

    public DogsFilterSpec(
        Guid? breedId, Guid? kennelId, DogStatus? status, string? search,
        string? sortBy, bool descending, int page, int pageSize)
    {
        Query.Include(d => d.Breed).Include(d => d.Kennel);

        Query.Where(d => d.BreedId == breedId!.Value, breedId.HasValue);
        Query.Where(d => d.KennelId == kennelId!.Value, kennelId.HasValue);
        Query.Where(d => d.Status == status!.Value, status.HasValue);
        Query.Where(d => d.Name.Contains(search!), !string.IsNullOrWhiteSpace(search));

        // сортування лише за дозволеними полями; Id — другий ключ для стабільної пагінації
        Expression<Func<DogEntity, object?>> keySelector = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "color" => d => d.Color,
            "taillength" => d => d.TailLength,
            "weight" => d => d.Weight,
            "createdat" => d => d.CreatedAt,
            _ => d => d.Name
        };

        var ordered = descending ? Query.OrderByDescending(keySelector) : Query.OrderBy(keySelector);
        ordered.ThenBy(d => d.Id);

        Query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
