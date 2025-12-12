using FluentResults;
using FluentValidation;
using Mediator;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool Available) : ICommand<Result>;

public sealed class Validator : AbstractValidator<UpdateProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");

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
