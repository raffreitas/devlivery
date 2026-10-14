using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Features.Expenses.Commands.CreateCategory;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.CreateCategory;

[Trait("Category", "Unit Tests")]
public sealed class CreateCategoryHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _handler = new CreateCategoryHandler(_categoryRepository, _unitOfWork, TestMocks.TenantAccessor(_tenantId));
    }

    [Theory]
    [InlineData(false, "Já existe uma categoria com o nome 'Operacional'.")]
    [InlineData(true, "Já existe uma subcategoria com o nome 'Operacional'.")]
    public async Task GivenDuplicateName_WhenCreating_ThenReturnsValidationErrorWithoutPersisting(
        bool isSubcategory, string expectedMessage)
    {
        // Arrange
        Guid? parentId = isSubcategory ? Guid.NewGuid() : null;
        _categoryRepository.ExistsWithNameAsync("Operacional", parentId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new CreateCategoryCommand("Operacional", parentId),
            CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<ValidationError>().Message.ShouldBe(expectedMessage);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenMissingOrInactiveParent_WhenCreatingSubcategory_ThenReturnsNotFoundError(bool parentExists)
    {
        // Arrange
        var parentId = Guid.NewGuid();
        if (parentExists)
            _categoryRepository.GetByIdAsync(parentId, Arg.Any<CancellationToken>())
                .Returns(new CategoryBuilder().AsInactive().Build());
        else
            _categoryRepository.GetByIdAsync(parentId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new CreateCategoryCommand("Insumos", parentId),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenNoParent_WhenCreating_ThenAddsTopLevelCategoryForCurrentTenant()
    {
        // Arrange
        Category? addedCategory = null;
        await _categoryRepository.AddAsync(Arg.Do<Category>(c => addedCategory = c), Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.HandleAsync(new CreateCategoryCommand("Operacional"), CancellationToken.None);

        // Assert
        addedCategory.ShouldNotBeNull();
        addedCategory.Name.ShouldBe("Operacional");
        addedCategory.EstablishmentId.ShouldBe(_tenantId);
        addedCategory.ParentCategoryId.ShouldBeNull();
        result.Value.CategoryId.ShouldBe(addedCategory.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenActiveParent_WhenCreatingSubcategory_ThenAttachesItToParentAndCommits()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        _categoryRepository.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

        // Act
        var result = await _handler.HandleAsync(new CreateCategoryCommand("Insumos", parent.Id),
            CancellationToken.None);

        // Assert
        var subcategory = parent.Subcategories.ShouldHaveSingleItem();
        subcategory.Name.ShouldBe("Insumos");
        subcategory.EstablishmentId.ShouldBe(_tenantId);
        subcategory.ParentCategoryId.ShouldBe(parent.Id);
        result.Value.CategoryId.ShouldBe(subcategory.Id);
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}