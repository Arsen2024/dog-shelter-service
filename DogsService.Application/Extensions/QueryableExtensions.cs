using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DogsService.Application.Extensions;

public static class QueryableExtensions
{
    public static Task<T?> FirstOrDefaultSafeAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
        )
    {
        if (source.Provider is IAsyncQueryProvider)
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source, predicate, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        var result = source.FirstOrDefault(predicate.Compile());
        return Task.FromResult(result);
    }

    public static Task<List<T>> ToListSafeAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default
        )
    {
        if (source.Provider is IAsyncQueryProvider)
            return EntityFrameworkQueryableExtensions.ToListAsync(source, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        var result = source.ToList<T>();
        return Task.FromResult(result);
    }
}
