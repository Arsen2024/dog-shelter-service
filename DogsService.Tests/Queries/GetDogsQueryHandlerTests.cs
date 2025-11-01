using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Application.Queries.Dog;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Tests.Queries
{
    public class GetDogsQueryHandlerTests
    {
        [Fact]
        public async void Handle_Should_ReturnSortedAndPagedDogs()
        {
            // Arrange
            var dogs = new List<DogEntity>
            {
                new DogEntity { Name = "B", Weight = 10 },
                new DogEntity { Name = "A", Weight = 20 },
                new DogEntity { Name = "C", Weight = 15 },
            }.AsQueryable();

            var repository = new Mock<IDogRepository>();
            repository.Setup(r => r.Query()).Returns(dogs);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.Dogs).Returns(repository.Object);

            var mapper = new MapperConfiguration(cfg => 
                cfg.CreateMap<DogEntity, DogDto>()).CreateMapper();

            var handler = new GetDogsQueryHandler(mapper, unitOfWork.Object);
            var query = new GetDogsQuery(
                "Weight",
                "desc",
                1,
                2
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result.First().Name);
        }
    }
}
