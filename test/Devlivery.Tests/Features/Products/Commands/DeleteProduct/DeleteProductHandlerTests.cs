using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Products.Commands.DeleteProduct;
using Devlivery.Shared.Application.Errors;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.DeleteProduct;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class DeleteProductHandlerTests(ProductsUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Devlivery.Features.Products.Domain.Product?)null);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Return_DomainRuleError_When_Product_Is_In_Use()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is DomainRuleError);
    }

    [Fact]
    public async Task Handle_Should_Remove_Product_When_Not_In_Use()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Call_Remove_On_Repository()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: product.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        productRepository.Received(1).Remove(product);
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: product.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_DeleteProductResponse_When_Successful()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        orderRepository.ExistsItemWithProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new DeleteProductHandler(productRepository, unitOfWork, orderRepository);

        var command = new DeleteProductCommand(Id: product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<DeleteProductResponse>();
    }
}