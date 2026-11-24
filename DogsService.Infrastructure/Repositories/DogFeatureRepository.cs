using DogsService.Domain.Entities;
using DogsService.Domain.Models;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.Repositories;

/// <summary>Репозиторій проміжної сутності M:N. Складений ключ, тому GetByIdAsync(Guid) не підтримується.</summary>
public class DogFeatureRepository(AppDbContext context) : EfRepository<DogFeatureEntity>(context), IDogFeatureRepository
{
    public override Task<DogFeatureEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        throw new NotSupportedException("DogFeature has a composite key. Use GetAsync(dogId, featureId).");

    public Task<DogFeatureEntity?> GetAsync(Guid dogId, Guid featureId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(df => df.DogId == dogId && df.FeatureId == featureId, ct);

    // LINQ to Entities через проміжну сутність: фільтр і сортування за її полем AssignedAt — на рівні БД
    public async Task<IReadOnlyList<FeatureAssignmentModel>> GetAssignmentsAddedAfterAsync(
        Guid featureId, DateTime? addedAfter, CancellationToken ct = default)
    {
        var query = Set.Where(df => df.FeatureId == featureId);

        if (addedAfter.HasValue)
            query = query.Where(df => df.AssignedAt > addedAfter.Value);

        return await query
            .OrderBy(df => df.AssignedAt)
            .Select(df => new FeatureAssignmentModel(
                df.DogId, df.Dog.Name, df.Dog.Breed.Name, df.AssignedAt, df.Notes))
            .ToListAsync(ct);
    }
}
