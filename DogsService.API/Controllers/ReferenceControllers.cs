using DogsService.Application.DTOs;
using DogsService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DogsService.API.Controllers;

[ApiController]
[Route("api/breeds")]
public class BreedsController(IBreedService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BreedDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    /// <summary>
    /// Порода. includeDogs=false — один запит; includeDogs=true — Explicit Loading:
    /// порода і собаки завантажуються двома окремими запитами.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BreedWithDogsDto>> GetById(Guid id, [FromQuery] bool includeDogs, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, includeDogs, ct));
}

[ApiController]
[Route("api/kennels")]
public class KennelsController(IKennelService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<KennelDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));
}

[ApiController]
[Route("api/features")]
public class FeaturesController(IFeatureService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FeatureDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    /// <summary>Собаки з характеристикою, додані після дати (запит через проміжну сутність DogFeature, поле AssignedAt).</summary>
    [HttpGet("{id:guid}/dogs")]
    public async Task<ActionResult<List<FeatureAssignmentDto>>> GetDogs(
        Guid id, [FromQuery] DateTime? addedAfter, CancellationToken ct) =>
        Ok(await service.GetDogsAddedAfterAsync(id, addedAfter, ct));
}
