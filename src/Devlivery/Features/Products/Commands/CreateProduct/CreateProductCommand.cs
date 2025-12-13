using Devlivery.Shared.Extensions;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available) : ICommand<Result<CreateProductResponse>>
{
    public bool IsValid(out string[] errors)
    {
        var result = new CreateProductCommandValidator().Validate(this);
        errors = result.GetErrors();
        return result.IsValid;
    }
};

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(200).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(1000).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O campo '{PropertyName}' deve ser maior que {ComparisonValue}.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.")
            .MaximumLength(100).WithMessage("O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
    }
}