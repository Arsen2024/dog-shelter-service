using System.Data;
using AdoptionsService.Infrastructure.Abstractions;

namespace AdoptionsService.Infrastructure.Repositories;

public sealed class UnitOfWork(
    IDbSession session,
    IAdopterRepository adopters,
    IApplicationRepository applications,
    IRequirementRepository requirements,
    IDogRepository dogs,
    IApplicationStatusHistoryRepository history) : IUnitOfWork
{
    public IAdopterRepository Adopters => adopters;
    public IApplicationRepository Applications => applications;
    public IRequirementRepository Requirements => requirements;
    public IDogRepository Dogs => dogs;
    public IApplicationStatusHistoryRepository History => history;

    public Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default) =>
        session.BeginTransactionAsync(isolationLevel, ct);

    public Task CommitAsync(CancellationToken ct = default) => session.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default) => session.RollbackAsync(ct);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        await session.BeginTransactionAsync(isolationLevel, ct);

        try
        {
            var result = await action();
            await session.CommitAsync(ct);
            return result;
        }
        catch
        {
            // Rollback не залежить від токена скасування запиту
            await session.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
