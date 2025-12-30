using Devlivery.Shared.Infrastructure.WebServer.Extensions;
using Devlivery.Shared.Infrastructure.WebServer.Models;
using Mediator;

namespace Devlivery.Features.Expenses.Commands.CreateCategory;

public static class CreateCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", Handle)
            .Produces<ApiResponse<CreateCategoryResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(CreateCategoryCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.ToApiResult(data => TypedResults.Created($"/api/expenses/categories/{data.CategoryId}", ApiResponse<CreateCategoryResponse>.Success(data)));
    }
}