using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext _dbContext,
    IDogRepository _dogRepository
    ) : IUnitOfWork
{
    public IDogRepository Dogs => _dogRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken);
}