using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Features.Products.Commands.CreateProduct;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Products.Commands.CreateProduct;

[Trait("Category", "Unit Tests")]
public sealed class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task GivenValidCommand_WhenCreatingProduct_ThenPersistsProductForCurrentTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        Product? addedProduct = null;
        await _productRepository.AddAsync(Arg.Do<Product>(p => addedProduct = p), Arg.Any<CancellationToken>());

        var handler = new CreateProductHandler(_productRepository, _unitOfWork, TestMocks.TenantAccessor(tenantId));
        var command = new CreateProductCommand("Pizza Calabresa", "Molho, calabresa e cebola", 49.90m, "Pizzas",
            Available: true);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        addedProduct.ShouldNotBeNull();
        addedProduct.Name.ShouldBe(command.Name);
        addedProduct.Description.ShouldBe(command.Description);
        addedProduct.Price.ShouldBe(command.Price);
        addedProduct.Category.ShouldBe(command.Category);
        addedProduct.Available.ShouldBeTrue();
        addedProduct.EstablishmentId.ShouldBe(tenantId);
        result.Value.ProductId.ShouldBe(addedProduct.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}