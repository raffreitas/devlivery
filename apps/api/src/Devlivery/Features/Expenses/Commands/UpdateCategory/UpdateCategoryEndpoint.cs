using Devlivery.Common.Endpoints;
using Devlivery.Common.Handlers;
using Devlivery.Infrastructure.Http.Extensions;
using Devlivery.Infrastructure.Http.Models;

using FluentResults;

namespace Devlivery.Features.Expenses.Commands.UpdateCategory;

public sealed class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("expenses/categories/{categoryId:guid}", Handle)
            .WithTags(Tags.Expenses)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid categoryId,
        UpdateCategoryRequest request,
        ICommandHandler<UpdateCategoryCommand, Result> handler,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: request.Name,
            IsActive: request.IsActive
        );

        var result = await handler.HandleAsync(command, ct);

        return result.ToNoContent();
    }
}

public sealed record UpdateCategoryRequest(string? Name, bool? IsActive);