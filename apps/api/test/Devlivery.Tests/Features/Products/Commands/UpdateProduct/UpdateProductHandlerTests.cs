using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Features.Products.Commands.UpdateProduct;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.UpdateProduct;

[Trait("Category", "Unit Tests")]
public sealed class UpdateProductHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _handler = new UpdateProductHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentProduct_WhenUpdating_ThenReturnsNotFoundErrorWithoutCommitting()
    {
        // Arrange
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new UpdateProductCommand(Guid.NewGuid(), "Pizza", "Descrição", 10m, "Pizzas", true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExistingProduct_WhenUpdating_ThenAppliesChangesAndCommits()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var command = new UpdateProductCommand(product.Id, "Pizza Portuguesa", "Presunto, ovo e cebola", 54.90m,
            "Pizzas Especiais", Available: true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Pizza Portuguesa");
        product.Description.ShouldBe("Presunto, ovo e cebola");
        product.Price.ShouldBe(54.90m);
        product.Category.ShouldBe("Pizzas Especiais");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task GivenProductAvailability_WhenUpdatingWithDifferentAvailability_ThenProductFollowsCommand(
        bool currentlyAvailable, bool requestedAvailability)
    {
        // Arrange
        var builder = new ProductBuilder();
        var product = (currentlyAvailable ? builder : builder.AsUnavailable()).Build();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var command = new UpdateProductCommand(product.Id, product.Name, product.Description, product.Price,
            product.Category, requestedAvailability);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        product.Available.ShouldBe(requestedAvailability);
    }
}