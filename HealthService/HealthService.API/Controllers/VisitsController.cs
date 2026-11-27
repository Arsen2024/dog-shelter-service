using HealthService.API.Contracts;
using HealthService.Application.Commands;
using HealthService.Application.DTOs;
using HealthService.Application.Queries;
using HealthService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Controllers;

/// <summary>Контролер лише передає команди й запити в ISender; бізнес-логіки й доступу до БД тут немає.</summary>
[ApiController]
[Route("api/visits")]
public class VisitsController(ISender sender) : ControllerBase
{
    /// <summary>Список візитів: фільтри dogId, type, текстовий пошук search, пагінація page/pageSize.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<VisitListItemDto>>> GetList(
        [FromQuery] GetVisitsListQuery query, CancellationToken ct) =>
        Ok(await sender.Send(query, ct));

    [HttpGet("by-dog/{dogId:guid}")]
    public async Task<ActionResult<PagedResult<VisitListItemDto>>> GetByDog(
        Guid dogId, [FromQuery] VisitType? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new GetVisitsListQuery(dogId, type, null, page, pageSize), ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<VisitDto>> GetById(string id, CancellationToken ct) =>
        Ok(await sender.Send(new GetVisitByIdQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<VisitDto>> Create([FromBody] CreateVisitCommand command, CancellationToken ct)
    {
        var created = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оновлення з перевіркою версії: Version має збігатися з поточною, інакше 409.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VisitDto>> Update(string id, [FromBody] UpdateVisitRequest request, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateVisitCommand(
            id, request.Version, request.Diagnosis, request.Symptoms, request.Recommendations,
            request.OperationNotes, request.CostAmount, request.CostCurrency), ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await sender.Send(new DeleteVisitCommand(id), ct);
        return NoContent();
    }
}
