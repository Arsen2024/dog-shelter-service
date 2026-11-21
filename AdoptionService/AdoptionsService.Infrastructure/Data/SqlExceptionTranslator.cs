using AdoptionsService.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

/// <summary>
/// Перетворює помилки SQL Server (THROW 5000x із збережених процедур, порушення обмежень)
/// на доменні винятки, які middleware перетворює на HTTP 404 / 409.
/// </summary>
public static class SqlExceptionTranslator
{
    public static Exception Translate(SqlException ex) => ex.Number switch
    {
        50001 or 50002 or 50003 => new NotFoundException(ex.Message),
        >= 50004 and <= 50010 or 50020 => new BusinessRuleException(ex.Message),
        2627 or 2601 => new BusinessRuleException("A record with the same unique value already exists."),
        547 => new BusinessRuleException("The operation violates a database constraint."),
        _ => ex
    };
}
