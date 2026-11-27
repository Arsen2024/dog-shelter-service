using HealthService.Application.Exceptions;
using HealthService.Domain.Exceptions;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Mongo;

/// <summary>
/// Перетворює MongoDB-специфічні винятки на винятки шарів Application/Domain, щоб WebApi не залежав від драйвера:
///   MongoWriteException / MongoBulkWriteException з DuplicateKey → ConflictException (409);
///   MongoConnectionException, TimeoutException (вибір сервера) → ServiceUnavailableException (503).
/// </summary>
public static class MongoGuard
{
    public static async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ConflictException("A document with the same unique key already exists.");
        }
        catch (MongoBulkWriteException ex) when (ex.WriteErrors.Any(e => e.Category == ServerErrorCategory.DuplicateKey))
        {
            throw new ConflictException("A document with the same unique key already exists.");
        }
        catch (Exception ex) when (ex is MongoConnectionException or TimeoutException)
        {
            throw new ServiceUnavailableException("MongoDB is unavailable. Try again later.", ex);
        }
    }

    public static async Task RunAsync(Func<Task> action)
    {
        await RunAsync(async () =>
        {
            await action();
            return true;
        });
    }
}
