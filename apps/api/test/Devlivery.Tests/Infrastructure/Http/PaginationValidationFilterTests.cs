using Devlivery.Infrastructure.Http.Filters;
using Devlivery.Infrastructure.Http.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Http;

[Trait("Category", "Unit Tests")]
public sealed class PaginationValidationFilterTests
{
    [Theory]
    [InlineData(0, 50)]
    [InlineData(-1, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 50)]
    public async Task GivenInvalidPagination_WhenInvokingFilter_ThenReturnsBadRequestWithoutCallingEndpoint(int page, int size)
    {
        // Arrange
        var filter = new PaginationValidationFilter();
        var context = EndpointFilterInvocationContext.Create(new DefaultHttpContext(), new PaginationParameters(page, size));
        var called = false;

        // Act
        var response = await filter.InvokeAsync(context, _ =>
        {
            called = true;
            return ValueTask.FromResult<object?>("ok");
        });

        // Assert
        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Title.ShouldBe("Requisição inválida");
        problem.ProblemDetails.Detail.ShouldBe("Os dados enviados na requisição estão em um formato inválido.");
        called.ShouldBeFalse();
    }

    [Theory]
    [InlineData(1, 50)]
    [InlineData(1, 100)]
    [InlineData(42949673, 50)]
    public async Task GivenValidPagination_WhenInvokingFilter_ThenCallsEndpoint(int page, int size)
    {
        // Arrange
        var filter = new PaginationValidationFilter();
        var context = EndpointFilterInvocationContext.Create(new DefaultHttpContext(), new PaginationParameters(page, size));

        // Act
        var response = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("ok"));

        // Assert
        response.ShouldBe("ok");
    }

    [Fact]
    public async Task GivenEndpointWithoutPagination_WhenInvokingFilter_ThenCallsEndpoint()
    {
        // Arrange
        var filter = new PaginationValidationFilter();
        var context = EndpointFilterInvocationContext.Create(new DefaultHttpContext(), "other parameter");

        // Act
        var response = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("ok"));

        // Assert
        response.ShouldBe("ok");
    }
}
