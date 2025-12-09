using FluentValidation;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionById;

public sealed class GetCashSessionByIdValidator : AbstractValidator<GetCashSessionByIdQuery>
{
    public GetCashSessionByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O id do caixa é obrigatório.");
    }
}
