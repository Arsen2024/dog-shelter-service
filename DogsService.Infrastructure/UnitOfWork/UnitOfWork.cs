using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext _dbContext,
    IDogRepository _dogRepository,
    IBreedRepository _breedRepository,
    IKennelRepository _kennelRepository
    ) : IUnitOfWork
{
    public IDogRepository Dogs => _dogRepository;
    public IBreedRepository Breeds => _breedRepository;
    public IKennelRepository Kennels => _kennelRepository;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
