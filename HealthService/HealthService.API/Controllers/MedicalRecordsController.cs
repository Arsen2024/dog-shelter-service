using HealthService.API.Contracts;
using HealthService.Application.Commands;
using HealthService.Application.DTOs;
using HealthService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Controllers;

[ApiController]
[Route("api/medical-records")]
public class MedicalRecordsController(ISender sender) : ControllerBase
{
    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<ActionResult<MedicalRecordDto>> GetByDog(Guid dogId, CancellationToken ct) =>
        Ok(await sender.Send(new GetMedicalRecordByDogIdQuery(dogId), ct));

    [HttpPost]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordCommand command, CancellationToken ct)
    {
        var created = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetByDog), new { dogId = created.DogId }, created);
    }

    [HttpPost("by-dog/{dogId:guid}/allergies")]
    public async Task<IActionResult> AddAllergy(Guid dogId, [FromBody] AddAllergyRequest request, CancellationToken ct)
    {
        await sender.Send(new AddAllergyCommand(dogId, request.Substance, request.Severity, request.NotedAt), ct);
        return NoContent();
    }

    [HttpPost("by-dog/{dogId:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid dogId, [FromBody] AddNoteRequest request, CancellationToken ct)
    {
        await sender.Send(new AddMedicalNoteCommand(dogId, request.Text), ct);
        return NoContent();
    }
}
