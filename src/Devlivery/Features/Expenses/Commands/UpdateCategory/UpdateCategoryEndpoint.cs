using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;

using Mediator;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Devlivery.Features.Expenses.Commands.UpdateCategory;

public static class UpdateCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{categoryId:guid}", Handle)
            .Produces<ApiResponse>(StatusCodes.Status204NoContent)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, BadRequest<ApiResponse>, NotFound<ApiResponse>>> Handle(
        Guid categoryId,
        UpdateCategoryRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: request.Name,
            IsActive: request.IsActive
        );

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

public sealed record UpdateCategoryRequest(
    string? Name,
    bool? IsActive);