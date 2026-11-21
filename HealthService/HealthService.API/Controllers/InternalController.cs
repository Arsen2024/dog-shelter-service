using HealthService.API.Contracts;
using HealthService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalController(IMediator _mediator) : ControllerBase
{
    [HttpPut("dogs/{dogId:guid}/name")]
    public async Task<IActionResult> Rename(Guid dogId, [FromBody] RenameDogRequest request)
    {
        await _mediator.Send(new RenameDogCommand(dogId, request.Name));
        return NoContent();
    }
}
