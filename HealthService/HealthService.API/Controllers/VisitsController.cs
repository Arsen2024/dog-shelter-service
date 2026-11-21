using HealthService.Application.Commands;
using HealthService.Application.Queries;
using HealthService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HealthService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/visits")]
public class VisitsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) =>
        Ok(await _mediator.Send(new GetVisitByIdQuery(id)));

    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<IActionResult> GetByDog(
        Guid dogId, [FromQuery] VisitType? type, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _mediator.Send(new GetVisitsByDogIdQuery(dogId, type, pageNumber, pageSize)));

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordVisitCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
