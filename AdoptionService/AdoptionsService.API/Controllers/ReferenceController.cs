using AdoptionsService.API.Contracts;
using AdoptionsService.Application.Commands;
using AdoptionsService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdoptionsService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api")]
public class ReferenceController(IMediator _mediator) : ControllerBase
{
    [HttpGet("requirements")]
    public async Task<IActionResult> GetRequirements() =>
        Ok(await _mediator.Send(new GetRequirementsQuery()));

    /// <summary>Локальна копія собак (читання).</summary>
    [HttpGet("dogs")]
    public async Task<IActionResult> GetDogs([FromQuery] string? status) =>
        Ok(await _mediator.Send(new GetDogsQuery(status)));

    /// <summary>Імітація події DogUpdated із DogsService (у Лаб. 08 замінюється обробником повідомлень).</summary>
    [HttpPut("internal/dogs/{id:guid}")]
    public async Task<IActionResult> SyncDog(Guid id, [FromBody] SyncDogRequest request)
    {
        await _mediator.Send(new SyncDogCommand(id, request.Name, request.Status));
        return NoContent();
    }
}
