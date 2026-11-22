using AdoptionsService.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

/// <summary>
/// SqlException → доменні винятки (коди THROW наших процедур з Лаб. 01):
///   50001-50003 (не знайдено)                     → NotFoundException
///   50004-50010, 50020 (правила бізнес-логіки)    → BusinessConflictException
///   2601 / 2627 (UNIQUE), 547 (FK/CHECK)          → BusinessConflictException
/// Решта помилок лишаються як є (HTTP 500 без деталей).
/// </summary>
public static class SqlExceptionTranslator
{
    public static Exception Translate(SqlException ex) => ex.Number switch
    {
        50001 or 50002 or 50003 => new NotFoundException(ex.Message),
        >= 50004 and <= 50010 or 50020 => new BusinessConflictException(ex.Message),
        2601 or 2627 => new BusinessConflictException("A record with the same unique value already exists."),
        547 => new BusinessConflictException("The operation violates a database constraint."),
        _ => ex
    };
}
