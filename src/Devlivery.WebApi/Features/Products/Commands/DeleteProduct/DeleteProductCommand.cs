using FluentValidation;

namespace Devlivery.WebApi.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id);

public sealed class Validator : AbstractValidator<DeleteProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}
