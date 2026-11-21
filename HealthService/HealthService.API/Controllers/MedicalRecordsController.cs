using HealthService.API.Contracts;
using HealthService.Application.Commands;
using HealthService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HealthService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/medical-records")]
public class MedicalRecordsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<IActionResult> GetByDog(Guid dogId) =>
        Ok(await _mediator.Send(new GetMedicalRecordByDogIdQuery(dogId)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByDog), new { dogId = command.DogId }, new { id });
    }

    [HttpPost("by-dog/{dogId:guid}/allergies")]
    public async Task<IActionResult> AddAllergy(Guid dogId, [FromBody] AddAllergyRequest request)
    {
        await _mediator.Send(new AddAllergyCommand(dogId, request.Substance, request.Severity, request.NotedAt));
        return NoContent();
    }

    [HttpPost("by-dog/{dogId:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid dogId, [FromBody] AddNoteRequest request)
    {
        await _mediator.Send(new AddMedicalNoteCommand(dogId, request.Text));
        return NoContent();
    }
}
