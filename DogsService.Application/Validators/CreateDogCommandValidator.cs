using DogsService.Application.Commands.Dog;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Validators
{
    public class CreateDogCommandValidator : AbstractValidator<CreateDogCommand>
    {
        public CreateDogCommandValidator() { 
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Colors)
                .NotEmpty().WithMessage("At least one color is required");

            RuleFor(x => x.TailLength)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tail length cannot be negative");

            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than zero");
        }
    }
}
