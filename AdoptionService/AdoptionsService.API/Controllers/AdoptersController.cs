using AdoptionsService.Application.DTOs;
using AdoptionsService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionsService.API.Controllers;

[ApiController]
[Route("api/adopters")]
public class AdoptersController(IAdopterService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdopterDto>>> GetAll([FromQuery] PageQuery query, CancellationToken ct) =>
        Ok(await service.GetPageAsync(query.PageNumber, query.PageSize, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdopterDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<AdopterDto>> Create([FromBody] CreateAdopterDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdopterDto>> Update(Guid id, [FromBody] UpdateAdopterDto dto, CancellationToken ct) =>
        Ok(await service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
