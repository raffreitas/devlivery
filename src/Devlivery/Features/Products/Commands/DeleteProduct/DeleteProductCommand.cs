using FluentResults;
using FluentValidation;
using Mediator;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result<DeleteProductResponse>>;

public sealed class Validator : AbstractValidator<DeleteProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}
