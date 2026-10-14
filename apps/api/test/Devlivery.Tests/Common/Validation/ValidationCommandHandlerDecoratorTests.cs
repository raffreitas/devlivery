using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Common.Validation;

using FluentResults;

using FluentValidation;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Common.Validation;

[Trait("Category", "Unit Tests")]
public sealed class ValidationCommandHandlerDecoratorTests
{
    public sealed record TestCommand(string Name, int Quantity);

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Nome é obrigatório.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
        }
    }

    [Fact]
    public async Task HandleAsync_Should_Return_ValidationError_And_Skip_Inner_When_Invalid()
    {
        // Arrange
        var inner = Substitute.For<ICommandHandler<TestCommand, Result>>();
        var decorator = new ValidationCommandHandlerDecorator<TestCommand, Result>(inner, [new TestCommandValidator()]);

        // Act
        var result = await decorator.HandleAsync(new TestCommand("", 0), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var error = result.Errors.Single().ShouldBeOfType<ValidationError>();
        error.Errors.Select(e => e.Field).ShouldBe(["Name", "Quantity"]);
        await inner.DidNotReceive().HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Typed_ValidationError_When_Invalid_And_Response_Has_Value()
    {
        // Arrange
        var inner = Substitute.For<ICommandHandler<TestCommand, Result<Guid>>>();
        var decorator = new ValidationCommandHandlerDecorator<TestCommand, Result<Guid>>(
            inner, [new TestCommandValidator()]);

        // Act
        var result = await decorator.HandleAsync(new TestCommand("Pizza", 0), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Single().ShouldBeOfType<ValidationError>()
            .Errors.Single().Message.ShouldBe("Quantidade deve ser maior que zero.");
        await inner.DidNotReceive().HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Call_Inner_When_Valid()
    {
        // Arrange
        var command = new TestCommand("Pizza", 2);
        var expected = Result.Ok(Guid.NewGuid());
        var inner = Substitute.For<ICommandHandler<TestCommand, Result<Guid>>>();
        inner.HandleAsync(command, Arg.Any<CancellationToken>()).Returns(expected);
        var decorator = new ValidationCommandHandlerDecorator<TestCommand, Result<Guid>>(
            inner, [new TestCommandValidator()]);

        // Act
        var result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(expected);
        await inner.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Should_Call_Inner_When_No_Validators()
    {
        // Arrange
        var command = new TestCommand("", 0);
        var inner = Substitute.For<ICommandHandler<TestCommand, Result>>();
        inner.HandleAsync(command, Arg.Any<CancellationToken>()).Returns(Result.Ok());
        var decorator = new ValidationCommandHandlerDecorator<TestCommand, Result>(inner, []);

        // Act
        var result = await decorator.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await inner.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
    }
}