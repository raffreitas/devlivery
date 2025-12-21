using Devlivery.Features.Products.Commands.UpdateProduct;
using Devlivery.Shared.Application.Errors;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.UpdateProduct;

[Collection("Products Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class UpdateProductHandlerTests(ProductsUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Devlivery.Features.Products.Domain.Product?)null);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: Guid.NewGuid(),
            Name: "Produto Atualizado",
            Description: "Descrição atualizada",
            Price: 199.99m,
            Category: "Nova Categoria",
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_Should_Update_Product_Properties()
    {
        // Arrange
        var product = fixture.CreateProduct(
            name: "Produto Original",
            description: "Descrição Original",
            price: 99.99m,
            category: "Categoria Original",
            available: false
        );

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Produto Atualizado",
            Description: "Descrição Atualizada",
            Price: 199.99m,
            Category: "Nova Categoria",
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Produto Atualizado");
        product.Description.ShouldBe("Descrição Atualizada");
        product.Price.ShouldBe(199.99m);
        product.Category.ShouldBe("Nova Categoria");
    }

    [Fact]
    public async Task Handle_Should_Set_Product_As_Available_When_Available_Is_True()
    {
        // Arrange
        var product = fixture.CreateProduct(available: false);

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Category: product.Category,
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Available.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Set_Product_As_Unavailable_When_Available_Is_False()
    {
        // Arrange
        var product = fixture.CreateProduct(available: true);

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Category: product.Category,
            Available: false
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Available.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_Should_Call_Update_On_Repository()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Produto Atualizado",
            Description: "Descrição Atualizada",
            Price: 199.99m,
            Category: "Nova Categoria",
            Available: true
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        productRepository.Received(1).Update(product);
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Produto Atualizado",
            Description: "Descrição Atualizada",
            Price: 199.99m,
            Category: "Nova Categoria",
            Available: true
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Product_Is_Updated()
    {
        // Arrange
        var product = fixture.CreateProduct();

        var productRepository = fixture.CreateProductRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductHandler(productRepository, unitOfWork);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Produto Atualizado",
            Description: "Descrição Atualizada",
            Price: 199.99m,
            Category: "Nova Categoria",
            Available: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}