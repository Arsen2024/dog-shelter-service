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
    public async Task<IActionResult> GetAll([FromQuery] GetDogsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDogCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}