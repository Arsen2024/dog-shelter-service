using HealthService.API.Contracts;
using HealthService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalController(ISender sender) : ControllerBase
{
    /// <summary>Імітація події DogUpdated із DogsService: оновлює денормалізоване dogName (Лаб. 08 — обробник повідомлень).</summary>
    [HttpPut("dogs/{dogId:guid}/name")]
    public async Task<IActionResult> Rename(Guid dogId, [FromBody] RenameDogRequest request, CancellationToken ct)
    {
        await sender.Send(new RenameDogCommand(dogId, request.Name), ct);
        return NoContent();
    }
}
