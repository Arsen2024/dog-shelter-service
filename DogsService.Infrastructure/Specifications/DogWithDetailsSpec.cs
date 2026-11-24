using Ardalis.Specification;
using DogsService.Domain.Entities;

namespace DogsService.Infrastructure.Specifications;

/// <summary>Eager Loading: собака + порода + вольєр + профіль + характеристики (Include / ThenInclude).</summary>
public class DogWithDetailsSpec : Specification<DogEntity>
{
    public DogWithDetailsSpec(Guid id)
    {
        Query.Where(d => d.Id == id);

        Query.Include(d => d.Breed);
        Query.Include(d => d.Kennel);
        Query.Include(d => d.Profile);
        Query.Include(d => d.DogFeatures).ThenInclude(df => df.Feature);
    }
}
