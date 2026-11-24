using DogsService.Domain.Entities;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.Repositories;

public class BreedRepository(AppDbContext context) : EfRepository<BreedEntity>(context), IBreedRepository
{
    // EXPLICIT LOADING: два окремі запити
    public async Task<BreedEntity?> GetWithDogsAsync(Guid id, CancellationToken ct = default)
    {
        var breed = await GetByIdAsync(id, ct);          // запит №1: порода
        if (breed is null)
            return null;

        await LoadDogsAsync(breed, ct);                   // запит №2: собаки породи
        return breed;
    }

    public Task LoadDogsAsync(BreedEntity breed, CancellationToken ct = default) =>
        Context.Entry(breed).Collection(b => b.Dogs).LoadAsync(ct);
}
