using DogsService.API.Controllers;
using DogsService.Application.Commands.Dog;
using DogsService.Application.Commands.Dog.Create;
using DogsService.Application.Commands.Dog.Delete;
using DogsService.Application.DTOs.Dog;
using DogsService.Application.Queries.Dog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DogsService.Tests.Controllers;
public class DogsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DogsController _controller;

    public DogsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new DogsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResult_WithDogsList()
    {
        // Arrange
        var query = new GetDogsQuery("Name", "asc", 1, 10);
        var expectedDogs = new List<DogDto>
        {
            new DogDto { Name = "Bobik", Color = "Red", TailLength = 10, Weight = 20 },
            new DogDto { Name = "Sharik", Color = "Black", TailLength = 5, Weight = 15 }
        };
        _mediatorMock
                .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDogs);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedDogs, okResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnOkResult_WithCreatedDog()
    {
        // Arrange
        var command = new CreateDogCommand("Rex", new List<string> { "Black" }, 5, 20);

        _mediatorMock
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(Unit.Value, okResult.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDogExists()
    {
        // Arrange
        var dogId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteDogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(dogId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteDogCommand>(c => c.Id == dogId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldThrowException_WhenDogNotFound()
    {
        // Arrange
        var dogId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteDogCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception($"Dog with id {dogId} not found"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.Delete(dogId));
    }
}
