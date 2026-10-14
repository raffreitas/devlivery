using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Commands.CreateCashSession;

public sealed class CreateCashSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("cash-register/sessions", Handle)
            .WithTags(Tags.CashRegister)
            .Produces<ApiResource<CreateCashSessionResponse>>(StatusCodes.Status201Created)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateCashSessionCommand command,
        ICommandHandler<CreateCashSessionCommand, Result<CreateCashSessionResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return result.ToCreated(response => $"/api/cash-register/sessions/{response.Id}");
    }
}