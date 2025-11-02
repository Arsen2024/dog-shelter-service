using DogsService.Application.Commands.Dog;
using DogsService.Application.Exceptions;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using Moq;

namespace DogsService.Tests.Commands;

public class CreateDogCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ThrowException_When_DuplicateName()
    {
        // Arrange 
        var existingDog = new DogEntity { Name = "Barsik" };
        var repository = new Mock<IDogRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository.Setup(r => r.Query()).Returns(new List<DogEntity> { existingDog }.AsQueryable());
        unitOfWork.Setup(u => u.Dogs).Returns(repository.Object);

        var handler = new CreateDogCommandHandler(unitOfWork.Object);

        var command = new CreateDogCommand("Barsik", new List<string> { "Black" }, 10, 15);

        // Assert
        await Assert.ThrowsAsync<DuplicateDogNameException>(() => handler.Handle(command, CancellationToken.None));
    }
}