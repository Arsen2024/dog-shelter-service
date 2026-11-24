using DogsService.Application.DTOs;
using DogsService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DogsService.API.Controllers;

[ApiController]
[Route("api/dogs")]
public class DogsController(IDogService service) : ControllerBase
{
    /// <summary>
    /// Список собак: пагінація (page, pageSize), фільтри (breedId, kennelId, status, search),
    /// сортування (sortBy, sortDir). Фільтрація, сортування й пагінація виконуються в БД.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<DogDto>>> GetPage([FromQuery] DogListQuery query, CancellationToken ct) =>
        Ok(await service.GetPageAsync(query, ct));

    /// <summary>Картка собаки (Eager Loading: один SQL-запит).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DogDetailsDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<DogDetailsDto>> Create([FromBody] CreateDogDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Зміна ваги (аналог «зміни ціни»).</summary>
    [HttpPut("{id:guid}/weight")]
    public async Task<IActionResult> ChangeWeight(Guid id, [FromBody] ChangeWeightDto dto, CancellationToken ct)
    {
        await service.ChangeWeightAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Зміна статусу (недопустимий перехід → 409).</summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto, CancellationToken ct)
    {
        await service.ChangeStatusAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Розміщення у вольєрі (аналог «списання залишку»: переповнений вольєр → 409).</summary>
    [HttpPut("{id:guid}/kennel")]
    public async Task<IActionResult> AssignKennel(Guid id, [FromBody] AssignKennelDto dto, CancellationToken ct)
    {
        await service.AssignKennelAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/kennel")]
    public async Task<IActionResult> RemoveFromKennel(Guid id, CancellationToken ct)
    {
        await service.RemoveFromKennelAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/features")]
    public async Task<IActionResult> AddFeature(Guid id, [FromBody] AddFeatureDto dto, CancellationToken ct)
    {
        await service.AddFeatureAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Soft delete.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
