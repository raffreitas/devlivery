using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public static class DeleteCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{categoryId:guid}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid categoryId,
        ICommandHandler<DeleteCategoryCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new DeleteCategoryCommand(categoryId);
        var result = await handler.HandleAsync(command, ct);
        return result.ToNoContent();
    }
}