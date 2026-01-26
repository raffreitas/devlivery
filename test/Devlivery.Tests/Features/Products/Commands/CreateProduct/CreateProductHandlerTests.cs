using Devlivery.Domain.Aggregates.Products;
using Devlivery.Features.Products.Commands.CreateProduct;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.CreateProduct;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateProductHandlerTests(ProductsUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Create_Product_With_Correct_Properties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(tenantId);
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);

        var command = new CreateProductCommand(
            Name: "Produto Teste",
            Description: "Descrição do produto teste",
            Price: 99.99m,
            Category: "Eletrônicos",
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_Should_Call_AddAsync_On_Repository()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);

        var command = new CreateProductCommand(
            Name: "Produto Teste",
            Description: "Descrição do produto teste",
            Price: 99.99m,
            Category: "Eletrônicos",
            Available: true
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await productRepository.Received(1).AddAsync(
            Arg.Is<Product>(p =>
                p.Name == command.Name &&
                p.Description == command.Description &&
                p.Price == command.Price &&
                p.Category == command.Category &&
                p.Available == command.Available &&
                p.EstablishmentId == tenantAccessor.Tenant.Id
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);

        var command = new CreateProductCommand(
            Name: "Produto Teste",
            Description: "Descrição do produto teste",
            Price: 99.99m,
            Category: "Eletrônicos",
            Available: true
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_TenantId_From_TenantAccessor()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(expectedTenantId);
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);

        var command = new CreateProductCommand(
            Name: "Produto Teste",
            Description: "Descrição do produto teste",
            Price: 99.99m,
            Category: "Eletrônicos",
            Available: true
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await productRepository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.EstablishmentId == expectedTenantId),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_Should_Return_CreateProductResponse_With_ProductId()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var handler = new CreateProductHandler(productRepository, unitOfWork, tenantAccessor);

        var command = new CreateProductCommand(
            Name: "Produto Teste",
            Description: "Descrição do produto teste",
            Price: 99.99m,
            Category: "Eletrônicos",
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldNotBe(Guid.Empty);
    }
}