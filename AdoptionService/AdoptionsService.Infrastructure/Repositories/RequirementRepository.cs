using AdoptionsService.Domain.Entities;
using AdoptionsService.Infrastructure.Abstractions;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>Довідник вимог: лише спільний CRUD з BaseDapperRepository.</summary>
public class RequirementRepository(IDbSession session)
    : BaseDapperRepository<RequirementEntity>(session, "dbo.Requirements"), IRequirementRepository
{
}
