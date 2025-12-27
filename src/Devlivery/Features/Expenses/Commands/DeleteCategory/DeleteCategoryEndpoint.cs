using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public static class DeleteCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{categoryId:guid}", Handle)
            .Produces<ApiResponse>(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>, NotFound<ApiResponse>>> Handle(
        Guid categoryId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteCategoryCommand(categoryId);
        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return result.ToNoContent();
        }

        var error = result.GetError();
        if (error is NotFoundError)
        {
            return result.ToNotFound();
        }

        return result.ToBadRequest();
    }
}