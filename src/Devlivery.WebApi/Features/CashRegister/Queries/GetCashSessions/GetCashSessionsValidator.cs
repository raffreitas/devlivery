using FluentValidation;

namespace Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessions;

public sealed class GetCashSessionsValidator : AbstractValidator<GetCashSessionsQuery>
{
    public GetCashSessionsValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("A data final deve ser maior ou igual à data inicial.");
    }
}
