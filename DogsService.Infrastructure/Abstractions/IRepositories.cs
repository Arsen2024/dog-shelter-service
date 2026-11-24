using DogsService.Domain.Entities;
using DogsService.Domain.Models;

namespace DogsService.Infrastructure.Abstractions;

public interface IDogRepository : IGenericRepository<DogEntity>
{
    /// <summary>EAGER LOADING: картка собаки з породою, вольєром, профілем і характеристиками — одним запитом.</summary>
    Task<DogEntity?> GetDetailsAsync(Guid id, CancellationToken ct = default);
}

public interface IBreedRepository : IGenericRepository<BreedEntity>
{
    /// <summary>EXPLICIT LOADING: спершу порода (запит №1), потім собаки окремим запитом (№2) — лише коли потрібні.</summary>
    Task<BreedEntity?> GetWithDogsAsync(Guid id, CancellationToken ct = default);

    Task LoadDogsAsync(BreedEntity breed, CancellationToken ct = default);
}

public interface IKennelRepository : IGenericRepository<KennelEntity>
{
}

public interface IFeatureRepository : IGenericRepository<FeatureEntity>
{
}

public interface IDogFeatureRepository : IGenericRepository<DogFeatureEntity>
{
    Task<DogFeatureEntity?> GetAsync(Guid dogId, Guid featureId, CancellationToken ct = default);

    /// <summary>LINQ to Entities через проміжну сутність: собаки з характеристикою, додані після дати (поле AssignedAt).</summary>
    Task<IReadOnlyList<FeatureAssignmentModel>> GetAssignmentsAddedAfterAsync(
        Guid featureId, DateTime? addedAfter, CancellationToken ct = default);
}
