using AdoptionsService.API.Contracts;
using AdoptionsService.Application.Commands;
using AdoptionsService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdoptionsService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class ApplicationsController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetApplicationsQuery query) =>
        Ok(await _mediator.Send(query));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _mediator.Send(new GetApplicationDetailsQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/dogs")]
    public async Task<IActionResult> AddDog(Guid id, [FromBody] AddDogRequest request)
    {
        await _mediator.Send(new AddDogToApplicationCommand(id, request.DogId, request.Priority));
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        await _mediator.Send(new ChangeApplicationStatusCommand(id, request.NewStatus, request.Comment));
        return NoContent();
    }

    [HttpPost("{id:guid}/agreement")]
    public async Task<IActionResult> SignAgreement(Guid id, [FromBody] SignAgreementRequest request)
    {
        await _mediator.Send(new SignAgreementCommand(id, request.DogId, request.AgreementNumber, request.SignedBy, request.Terms));
        return NoContent();
    }

    [HttpPost("{id:guid}/requirements/{requirementId:guid}/fulfill")]
    public async Task<IActionResult> Fulfill(Guid id, Guid requirementId, [FromBody] FulfillRequirementRequest request)
    {
        await _mediator.Send(new FulfillRequirementCommand(id, requirementId, request.Notes));
        return NoContent();
    }
}
