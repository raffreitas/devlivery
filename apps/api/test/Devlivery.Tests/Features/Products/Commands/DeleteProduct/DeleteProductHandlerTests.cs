using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Features.Products.Commands.DeleteProduct;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.DeleteProduct;

[Trait("Category", "Unit Tests")]
public sealed class DeleteProductHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _handler = new DeleteProductHandler(_productRepository, _unitOfWork, _orderRepository);
    }

    [Fact]
    public async Task GivenNonexistentProduct_WhenDeleting_ThenReturnsNotFoundError()
    {
        // Arrange
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenProductUsedInOrders_WhenDeleting_ThenReturnsValidationErrorAndKeepsProduct()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        result.HasError<ValidationError>().ShouldBeTrue();
        await _productRepository.DidNotReceive().RemoveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenProductNotUsedInOrders_WhenDeleting_ThenRemovesProductAndCommits()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _productRepository.Received(1).RemoveAsync(product, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}