using DogsService.Application.Commands.Dog;
using DogsService.Application.Queries.Dog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DogsService.API.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class DogsController(IMediator _mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetDogsQuery query) => Ok(await _mediator.Send(query));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDogCommand command) => Ok(await _mediator.Send(command));
}