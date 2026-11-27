using HealthService.Application.Commands;
using HealthService.Application.DTOs;
using HealthService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Controllers;

[ApiController]
[Route("api/vaccinations")]
public class VaccinationsController(ISender sender) : ControllerBase
{
    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<ActionResult<List<VaccinationDto>>> GetByDog(Guid dogId, CancellationToken ct) =>
        Ok(await sender.Send(new GetVaccinationsByDogIdQuery(dogId), ct));

    /// <summary>Собаки, остання вакцинація яких прострочена (aggregation pipeline).</summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<OverdueVaccinationDto>>> Overdue(CancellationToken ct) =>
        Ok(await sender.Send(new GetOverdueVaccinationsQuery(), ct));

    [HttpPost]
    public async Task<ActionResult<VaccinationDto>> Record([FromBody] RecordVaccinationCommand command, CancellationToken ct)
    {
        var created = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetByDog), new { dogId = created.DogId }, created);
    }
}
