using DogsService.Application.Commands.Dog.Create;
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

    [Fact]
    public void Should_HaveError_When_NameIsNull()
    {
        var command = new CreateDogCommand(null, new List<string> { "Black" }, 5, 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_NotHaveError_When_TailLengthIsZero()
    {
        var command = new CreateDogCommand("Sharik", new List<string> { "Brown" }, 0, 15);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TailLength);
    }

    [Fact]
    public void Should_HaveError_When_WeightIsNegative()
    {
        var command = new CreateDogCommand("Tuzik", new List<string> { "White" }, 5, -3);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorMessage("Weight must be greater than zero");
    }

    [Fact]
    public void Should_NotHaveError_When_ColorsContainMultipleValues()
    {
        var command = new CreateDogCommand("Rex", new List<string> { "Black", "White", "Brown" }, 8, 25);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Colors);
    }

    [Fact]
    public void Should_NotHaveError_When_NameIsValidAndTrimmed()
    {
        var command = new CreateDogCommand("  Rex  ", new List<string> { "Gray" }, 7, 18);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}