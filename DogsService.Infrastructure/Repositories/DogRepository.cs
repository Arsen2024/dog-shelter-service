using DogsService.Domain.Entities;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;
using DogsService.Infrastructure.Specifications;

namespace DogsService.Infrastructure.Repositories;

public class DogRepository(AppDbContext context) : EfRepository<DogEntity>(context), IDogRepository
{
    // EAGER LOADING: Include / ThenInclude описані у специфікації → один SQL-запит із JOIN
    public Task<DogEntity?> GetDetailsAsync(Guid id, CancellationToken ct = default) =>
        FirstOrDefaultAsync(new DogWithDetailsSpec(id), ct);
}
