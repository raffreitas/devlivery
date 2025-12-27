using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.CreateCategory;

public static class CreateCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", Handle)
            .Produces<ApiResponse<CreateCategoryResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Created<ApiResponse<CreateCategoryResponse>>,
        BadRequest<ApiResponse<CreateCategoryResponse>>>> Handle(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? result.ToCreated($"/api/expenses/categories/{result.Value.CategoryId}")
            : result.ToBadRequest();
    }
}