using Asp.Versioning;
using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;
using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

public sealed class GetCashHistoryEndpoint : IEndpoint
{
    public ApiVersion Version => new(2, 0);

    public void MapEndpoint(IEndpointRouteBuilder app) => app.MapGet("cash-register/sessions", Handle)
        .WithTags(Tags.CashRegister).Produces<ApiResource<GetCashHistoryResponse>>()
        .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiProblemDetails>(StatusCodes.Status422UnprocessableEntity);

    private static async Task<IResult> Handle(DateOnly start, DateOnly end, [AsParameters] PaginationParameters pagination,
        IQueryHandler<GetCashHistoryQuery, Result<GetCashHistoryResult>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new(start, end, pagination.Page, pagination.PageSize), ct);
        if (result.IsFailed) return result.ToOk();
        return TypedResults.Ok(new ApiResource<GetCashHistoryResponse>(
            new(result.Value.Page.Items, result.Value.Summary), Metadata.FromPaginationResult(result.Value.Page)));
    }
}
