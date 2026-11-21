namespace DogsService.Domain.Interfaces;

public interface IUnitOfWork
{
    IDogRepository Dogs { get; }
    IBreedRepository Breeds { get; }
    IKennelRepository Kennels { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
