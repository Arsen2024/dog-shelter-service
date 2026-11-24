using Ardalis.Specification;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.Repositories;

/// <summary>Базова реалізація Generic Repository через DbSet&lt;T&gt;.</summary>
public class EfRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
{
    protected AppDbContext Context { get; } = context;

    protected DbSet<T> Set => Context.Set<T>();

    // FirstOrDefault (а не Find), щоб застосовувались глобальні query filters (soft delete)
    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);

    public void Delete(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        await WithSpecification(specification).ToListAsync(ct);

    public Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        WithSpecification(specification).FirstOrDefaultAsync(ct);

    // лише критерії (Where) — без Include, сортування та Skip/Take; саме це потрібно для totalCount
    public Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        WithSpecification(specification, evaluateCriteriaOnly: true).CountAsync(ct);

    public Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken ct = default) =>
        WithSpecification(specification, evaluateCriteriaOnly: true).AnyAsync(ct);

    /// <summary>Застосовує специфікацію до DbSet. Єдине місце, де це відбувається, — нові специфікації не потребують змін у репозиторії.</summary>
    protected IQueryable<T> WithSpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false) =>
        Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default
            .GetQuery(Set.AsQueryable(), specification, evaluateCriteriaOnly);
}
