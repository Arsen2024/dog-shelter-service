namespace DogsService.Domain.Interfaces;

public interface IUnitOfWork
{
    IDogRepository Dogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}