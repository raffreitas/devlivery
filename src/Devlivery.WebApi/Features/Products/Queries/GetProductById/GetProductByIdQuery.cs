using FluentValidation;

namespace Devlivery.WebApi.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id);

public sealed class Validator : AbstractValidator<GetProductByIdQuery>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}
