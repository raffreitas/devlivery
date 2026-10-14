using FluentValidation;

using Shouldly;

namespace Devlivery.Tests.Common.Validation;

[Trait("Category", "Unit Tests")]
public sealed class PortugueseLanguageManagerTests
{
    public enum Size { Small = 1 }

    public sealed record Sample(string? Name, string? Email, string? Code, decimal Price, decimal Fee, Size Size);

    private sealed class SampleValidator : AbstractValidator<Sample>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithName("Nome");
            RuleFor(x => x.Email).EmailAddress().WithName("E-mail");
            RuleFor(x => x.Code).MinimumLength(3).MaximumLength(5).WithName("Código");
            RuleFor(x => x.Price).GreaterThan(0).WithName("Preço");
            RuleFor(x => x.Fee).GreaterThanOrEqualTo(0).WithName("Taxa");
            RuleFor(x => x.Size).IsInEnum().WithName("Tamanho");
        }
    }

    // No setup needed: the module initializer configures the messages as soon as the API assembly loads.
    [Theory]
    [InlineData("Name", "O campo 'Nome' é obrigatório.")]
    [InlineData("Email", "O campo 'E-mail' deve ser um e-mail válido.")]
    [InlineData("Price", "O campo 'Preço' deve ser maior que 0.")]
    [InlineData("Fee", "O campo 'Taxa' deve ser maior ou igual a 0.")]
    [InlineData("Size", "O campo 'Tamanho' possui um valor inválido.")]
    public void GivenInvalidValue_WhenValidating_ThenReturnsStandardPortugueseMessage(string property, string expectedMessage)
    {
        // Arrange
        var sample = new Sample(Name: "", Email: "invalido", Code: "abcd", Price: 0, Fee: -1, Size: (Size)99);

        // Act
        var result = new SampleValidator().Validate(sample);

        // Assert
        result.Errors.Single(e => e.PropertyName == property).ErrorMessage.ShouldBe(expectedMessage);
    }

    [Theory]
    [InlineData("ab", "O campo 'Código' deve ter no mínimo 3 caracteres.")]
    [InlineData("abcdef", "O campo 'Código' deve ter no máximo 5 caracteres.")]
    public void GivenCodeOutsideLengthLimits_WhenValidating_ThenReturnsPortugueseLengthMessage(string code, string expectedMessage)
    {
        // Arrange
        var sample = new Sample(Name: "Pizza", Email: null, Code: code, Price: 1, Fee: 0, Size: Size.Small);

        // Act
        var result = new SampleValidator().Validate(sample);

        // Assert
        result.Errors.Single().ErrorMessage.ShouldBe(expectedMessage);
    }
}