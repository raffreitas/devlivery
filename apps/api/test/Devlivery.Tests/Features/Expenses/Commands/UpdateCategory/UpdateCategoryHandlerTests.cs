using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Expenses.Abstractions;
using Devlivery.Features.Expenses.Commands.UpdateCategory;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.Expenses.Commands.UpdateCategory;

[Trait("Category", "Unit Tests")]
public sealed class UpdateCategoryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _handler = new UpdateCategoryHandler(_categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentCategory_WhenUpdating_ThenReturnsNotFoundError()
    {
        // Arrange
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new UpdateCategoryCommand(Guid.NewGuid(), "Operação", null),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenNameUsedBySibling_WhenRenamingSubcategory_ThenReturnsValidationErrorKeepingName()
    {
        // Arrange
        var parent = new CategoryBuilder().Build();
        var subcategory = new CategoryBuilder().WithName("Insumos").WithParent(parent).Build();
        _categoryRepository.GetByIdAsync(subcategory.Id, Arg.Any<CancellationToken>()).Returns(subcategory);
        _categoryRepository.ExistsWithNameAsync("Embalagens", parent.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new UpdateCategoryCommand(subcategory.Id, "Embalagens", null),
            CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<ValidationError>()
            .Message.ShouldBe("Já existe uma subcategoria com o nome 'Embalagens'.");
        subcategory.Name.ShouldBe("Insumos");
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenUnchangedName_WhenUpdatingStatus_ThenSkipsNameUniquenessCheck()
    {
        // Arrange
        var category = new CategoryBuilder().WithName("Operacional").Build();
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _categoryRepository.ExistsWithNameAsync("Operacional", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.HandleAsync(new UpdateCategoryCommand(category.Id, "Operacional", false),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenAvailableName_WhenUpdating_ThenRenamesCategoryAndCommits()
    {
        // Arrange
        var category = new CategoryBuilder().WithName("Operacional").Build();
        _categoryRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);

        // Act
        var result = await _handler.HandleAsync(new UpdateCategoryCommand(category.Id, "Operação", null),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.Name.ShouldBe("Operação");
        category.IsActive.ShouldBeTrue();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}