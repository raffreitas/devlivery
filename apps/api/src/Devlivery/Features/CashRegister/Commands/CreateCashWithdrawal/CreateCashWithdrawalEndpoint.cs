using System.Globalization;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashWithdrawal;

public sealed class CreateCashWithdrawalEndpoint : IEndpoint
{
    internal sealed record CreateCashWithdrawalRequest(decimal Amount, string? Reason)
    {
        public CreateCashWithdrawalCommand ToCommand(Guid sessionId) => new(sessionId, Amount, Reason?.Trim() ?? "");
    }

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("cash-register/sessions/{sessionId:guid}/withdrawals", Handle)
            .WithIdempotency<CreateCashWithdrawalCommand, CashWithdrawalResponse>(
                "cash-withdrawal", BindIdempotency, onFirstSuccess: response => response.ToCreated())
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<CashWithdrawalResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResource<CashWithdrawalResponse>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ApiProblemDetails>(StatusCodes.Status422UnprocessableEntity);

    private static IdempotencyEndpointRequest<CreateCashWithdrawalCommand> BindIdempotency(EndpointFilterInvocationContext context)
    {
        var sessionId = context.GetArgument<Guid>(0);
        var request = context.GetArgument<CreateCashWithdrawalRequest>(1);
        var command = request.ToCommand(sessionId);
        // Keep this representation in sync with the backfill in MoveCashWithdrawalIdempotencyToApi.
        var content = string.Create(CultureInfo.InvariantCulture, $"{command.Amount:G29}\n{command.Reason}");
        return new(command, sessionId, content);
    }

    private static Task<Result<CashWithdrawalResponse>> Handle(
        Guid sessionId,
        CreateCashWithdrawalRequest request,
        ICommandHandler<CreateCashWithdrawalCommand, Result<CashWithdrawalResponse>> handler,
        CancellationToken ct) =>
        handler.HandleAsync(request.ToCommand(sessionId), ct);
}
