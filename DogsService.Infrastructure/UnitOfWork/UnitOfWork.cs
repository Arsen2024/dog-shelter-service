using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext _dbContext,
    IDogRepository _dogRepository
    ) : IUnitOfWork
{
    public IDogRepository Dogs => _dogRepository;

    // No need for async/await — avoids extra state machine allocation and improves perf.
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}