using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.DeleteCategory;

public sealed class DeleteCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("expenses/categories/{categoryId:guid}", Handle)
            .WithTags(Tags.Expenses)
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