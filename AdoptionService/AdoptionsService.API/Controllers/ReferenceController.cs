using AdoptionsService.Application.DTOs;
using AdoptionsService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionsService.API.Controllers;

[ApiController]
[Route("api")]
public class ReferenceController(IReferenceService service) : ControllerBase
{
    [HttpGet("requirements")]
    public async Task<ActionResult<List<RequirementDto>>> GetRequirements(CancellationToken ct) =>
        Ok(await service.GetRequirementsAsync(ct));

    /// <summary>Локальна копія собак із Каталогу.</summary>
    [HttpGet("dogs")]
    public async Task<ActionResult<List<DogDto>>> GetDogs([FromQuery] string? status, CancellationToken ct) =>
        Ok(await service.GetDogsAsync(status, ct));

    /// <summary>Імітація події DogUpdated із DogsService (у Лаб. 08 замінюється обробником повідомлень).</summary>
    [HttpPut("internal/dogs/{id:guid}")]
    public async Task<IActionResult> SyncDog(Guid id, [FromBody] SyncDogDto dto, CancellationToken ct)
    {
        await service.SyncDogAsync(id, dto, ct);
        return NoContent();
    }
}
