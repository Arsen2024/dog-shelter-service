using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Commands.Dog
{
    public record CreateDogCommand(string Name, List<string> Colors, int TailLength, int Weight) : IRequest<Unit>;
}
