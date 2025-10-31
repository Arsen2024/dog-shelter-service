using DogsService.Application.Exceptions;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Commands.Dog
{
    public class CreateDogCommandHandler(
        IUnitOfWork unitOfWork
        ) : IRequestHandler<CreateDogCommand, Unit>
    {
        public async Task<Unit> Handle(CreateDogCommand request, CancellationToken cancellationToken)
        {
            var dublicate = await unitOfWork.Dogs.Query()
                .FirstOrDefaultAsync(d => d.Name == request.Name, cancellationToken);

            if (dublicate != null)
                throw new DuplicateDogNameException(request.Name);

            var dog = new DogEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Color = string.Join(" & ", request.Colors.Select(c => c.Trim().ToLower())),
                TailLength = request.TailLength,
                Weight = request.Weight,
            };

            await unitOfWork.Dogs.CreateAsync(dog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
