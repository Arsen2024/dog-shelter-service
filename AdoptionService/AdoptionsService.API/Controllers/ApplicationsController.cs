using AdoptionsService.Application.DTOs;
using AdoptionsService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionsService.API.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController(IApplicationService service, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ApplicationListItemDto>>> GetAll([FromQuery] ApplicationListQuery query, CancellationToken ct) =>
        Ok(await service.GetPageAsync(query, ct));

    /// <summary>Заявка з собаками (multi-mapping: один запит).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    /// <summary>Повні деталі: заявник, договір, собаки, вимоги, історія (збережена процедура usp_GetApplicationDetails).</summary>
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<ApplicationDetailsDto>> GetDetails(Guid id, CancellationToken ct) =>
        Ok(await service.GetDetailsAsync(id, ct));

    /// <summary>Історія статусів (репозиторій на чистому ADO.NET).</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<List<StatusHistoryDto>>> GetHistory(Guid id, CancellationToken ct) =>
        Ok(await service.GetHistoryAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create([FromBody] CreateApplicationDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/dogs")]
    public async Task<IActionResult> AddDog(Guid id, [FromBody] AddDogDto dto, CancellationToken ct)
    {
        await service.AddDogAsync(id, dto, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto, CancellationToken ct)
    {
        await service.ChangeStatusAsync(id, dto, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/requirements/{requirementId:guid}/fulfill")]
    public async Task<IActionResult> FulfillRequirement(
        Guid id, Guid requirementId, [FromBody] FulfillRequirementDto dto, CancellationToken ct)
    {
        await service.FulfillRequirementAsync(id, requirementId, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Підписання договору: договір + статус Completed + запис в історію — в одній транзакції.
    /// simulateFailure=true (лише в Development) кидає помилку після всіх записів, щоб показати відкат.
    /// </summary>
    [HttpPost("{id:guid}/agreement")]
    public async Task<IActionResult> SignAgreement(
        Guid id, [FromBody] SignAgreementDto dto, [FromQuery] bool simulateFailure, CancellationToken ct)
    {
        await service.SignAgreementAsync(id, dto, simulateFailure && environment.IsDevelopment(), ct);
        return NoContent();
    }
}
