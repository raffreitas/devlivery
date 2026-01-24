using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}