using HealthService.Application.Commands;
using HealthService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HealthService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/vaccinations")]
public class VaccinationsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<IActionResult> GetByDog(Guid dogId) =>
        Ok(await _mediator.Send(new GetVaccinationsByDogIdQuery(dogId)));

    /// <summary>Собаки, остання вакцинація яких прострочена (aggregation pipeline).</summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> Overdue() =>
        Ok(await _mediator.Send(new GetOverdueVaccinationsQuery()));

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordVaccinationCommand command)
    {
        var id = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, new { id });
    }
}
