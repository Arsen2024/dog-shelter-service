using Ardalis.Specification;
using DogsService.Domain.Entities;

namespace DogsService.Infrastructure.Specifications;

/// <summary>Собаки у вольєрі — використовується для перевірки заповненості.</summary>
public class DogsByKennelSpec : Specification<DogEntity>
{
    public DogsByKennelSpec(Guid kennelId)
    {
        Query.Where(d => d.KennelId == kennelId);
    }
}
