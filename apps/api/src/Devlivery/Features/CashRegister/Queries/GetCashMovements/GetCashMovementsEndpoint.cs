using Asp.Versioning;

using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Common.Pagination;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.CashRegister.Queries.GetCashMovements;

public sealed class GetCashMovementsEndpoint : IEndpoint
{
    public ApiVersion Version => new(2, 0);

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("cash-register/sessions/{id:guid}/movements", Handle).WithTags(Tags.CashRegister)
            .Produces<ApiResource<GetCashMovementsResponse[]>>()
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(Guid id, [AsParameters] PaginationParameters pagination,
        IQueryHandler<GetCashMovementsQuery, Result<PaginatedResult<GetCashMovementsResponse>>> handler,
        CancellationToken ct) =>
        (await handler.HandleAsync(new GetCashMovementsQuery(id, pagination.Page, pagination.PageSize), ct)).ToOk();
}
