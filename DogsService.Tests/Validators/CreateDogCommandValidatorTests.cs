using DogsService.Application.Commands.Dog;
using DogsService.Application.Validators;
using FluentValidation.TestHelper;

namespace DogsService.Tests.Validators;

public class CreateDogCommandValidatorTests
{
    private readonly CreateDogCommandValidator _validator;

    public CreateDogCommandValidatorTests()
    {
        _validator = new CreateDogCommandValidator();
    }
    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var command = new CreateDogCommand("", new List<string> { "Red" }, 10, 20);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_HaveError_When_ColorsAreEmpty()
    {
        var command = new CreateDogCommand("Bobik", new List<string>(), 10, 20);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Colors)
            .WithErrorMessage("At least one color is required");
    }

    [Fact]
    public void Should_HaveError_When_TailLengthIsNegative()
    {
        var command = new CreateDogCommand("Bobik", new List<string> { "Red" }, -10, 20);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TailLength)
            .WithErrorMessage("Tail length cannot be negative");
    }

    [Fact]
    public void Should_HaveError_When_WeghtIsZeroOrNegative()
    {
        var command = new CreateDogCommand("Bobik", new List<string> { "Red" }, 10, 0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorMessage("Weight must be greater than zero");
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        var command = new CreateDogCommand("Bobik", new List<string> { "Red" }, 10, 20);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}