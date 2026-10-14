using FluentValidation;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available);

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithName("Nome");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000).WithName("Descrição");
        RuleFor(x => x.Price).GreaterThan(0).WithName("Preço");
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100).WithName("Categoria");
    }
}