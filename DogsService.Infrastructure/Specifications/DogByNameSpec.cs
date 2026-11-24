using Ardalis.Specification;
using DogsService.Domain.Entities;

namespace DogsService.Infrastructure.Specifications;

/// <summary>Перевірка унікальності імені (аналог унікальності SKU).</summary>
public class DogByNameSpec : Specification<DogEntity>
{
    public DogByNameSpec(string name)
    {
        Query.Where(d => d.Name == name);
    }
}
