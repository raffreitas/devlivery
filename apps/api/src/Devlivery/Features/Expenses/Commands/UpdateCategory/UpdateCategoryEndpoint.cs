using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using Mediator;

namespace Devlivery.Features.Expenses.Commands.UpdateCategory;

public static class UpdateCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{categoryId:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
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

        return result.ToNoContent();
    }
}

public sealed record UpdateCategoryRequest(string? Name, bool? IsActive);