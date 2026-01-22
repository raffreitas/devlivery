using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

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

    private static async Task<IResult> Handle(Guid categoryId, UpdateCategoryRequest request, ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: request.Name,
            IsActive: request.IsActive
        );

        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}

public sealed record UpdateCategoryRequest(string? Name, bool? IsActive);