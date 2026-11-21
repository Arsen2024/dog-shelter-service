using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Data;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

public class RequirementRepository(ISqlConnectionFactory factory) : SqlRepositoryBase(factory), IRequirementRepository
{
    public Task<List<RequirementEntity>> GetAllAsync(CancellationToken ct) =>
        RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync<RequirementEntity>(new CommandDefinition(
                "SELECT Id, Code, Title, Description, IsMandatory FROM dbo.Requirements ORDER BY Code",
                cancellationToken: ct));
            return rows.ToList();
        }, ct);
}
