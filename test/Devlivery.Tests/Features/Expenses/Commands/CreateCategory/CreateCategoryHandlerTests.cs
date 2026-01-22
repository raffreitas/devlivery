using Devlivery.Common.Errors;
using Devlivery.Features.Expenses.Commands.CreateCategory;
using Devlivery.Features.Expenses.Domain.Aggregates.Categories;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateCategory;

[Trait("Category", "Unit Tests")]
[Collection("Expenses Unit Tests")]
public sealed class CreateCategoryHandlerTests(ExpensesUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_WithDuplicateName_ReturnsValidationError()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.ExistsWithName(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var handler = new CreateCategoryHandler(categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateCategoryCommand("Nome");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is ValidationError);
    }

    [Fact]
    public async Task Handle_WithParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.ExistsWithName(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var handler = new CreateCategoryHandler(categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateCategoryCommand("Nome", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task Handle_WithoutParent_AddsCategory_And_ReturnsResponse()
    {
        // Arrange
        var categoryRepository = fixture.CreateCategoryRepositoryMock();
        categoryRepository.ExistsWithName(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var tenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(tenantId);

        var handler = new CreateCategoryHandler(categoryRepository, unitOfWork, tenantAccessor);

        var command = new CreateCategoryCommand("Nome");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.CategoryId.ShouldNotBe(Guid.Empty);
        await categoryRepository.Received().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}