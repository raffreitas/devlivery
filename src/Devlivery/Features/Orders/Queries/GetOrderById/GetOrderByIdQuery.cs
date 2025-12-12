using FluentValidation;

namespace Devlivery.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id);

public sealed class Validator : AbstractValidator<GetOrderByIdQuery>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}