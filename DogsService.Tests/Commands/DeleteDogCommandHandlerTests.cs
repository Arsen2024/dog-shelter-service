using DogsService.Application.Commands.Dog.Delete;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace DogsService.Tests.Commands;
public class DeleteDogCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_DeleteDog_When_DogExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var dogId = Guid.NewGuid();
        var dog = new DogEntity { Id = dogId, Name = "Rex", Color = "Blacl" };
        dbContext.Dogs.Add(dog);
        await dbContext.SaveChangesAsync();

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Dogs.Query()).Returns(dbContext.Dogs);
        unitOfWork.Setup(u => u.Dogs.Delete(It.IsAny<DogEntity>()))
                  .Callback<DogEntity>(d => dbContext.Dogs.Remove(d));
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .Returns(() => dbContext.SaveChangesAsync());

        var handler = new DeleteDogCommandHandler(unitOfWork.Object);
        var command = new DeleteDogCommand(dogId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.Null(await dbContext.Dogs.FindAsync(dogId));
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_DogNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Dogs.Query()).Returns(dbContext.Dogs);

        var handler = new DeleteDogCommandHandler(unitOfWork.Object);
        var command = new DeleteDogCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}
