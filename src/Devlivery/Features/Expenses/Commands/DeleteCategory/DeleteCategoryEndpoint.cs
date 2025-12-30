using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

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

    private static async Task<IResult> Handle(
        Guid categoryId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteCategoryCommand(categoryId);
        var result = await sender.Send(command, ct);

        return result.ToApiResult(TypedResults.NoContent);
    }
}