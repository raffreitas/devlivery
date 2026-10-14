using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashDeposit;

[Trait("Category", "Unit Tests")]
public sealed class CreateCashDepositHandlerTests
{
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCashDepositHandler _handler;

    public CreateCashDepositHandlerTests()
    {
        _handler = new CreateCashDepositHandler(
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId, "Maria Souza"),
            _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentSession_WhenDepositing_ThenReturnsNotFoundError()
    {
        // Arrange
        _cashSessionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new CreateCashDepositCommand(Guid.NewGuid(), 50m, null),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenClosedSession_WhenDepositing_ThenReturnsValidationErrorWithoutRecording()
    {
        // Arrange
        var session = new CashSessionBuilder().AsClosed().Build();
        _cashSessionRepository.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        var result = await _handler.HandleAsync(new CreateCashDepositCommand(session.Id, 50m, null),
            CancellationToken.None);

        // Assert
        result.HasError<ValidationError>().ShouldBeTrue();
        session.Movements.ShouldBeEmpty();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenOpenSession_WhenDepositing_ThenRecordsDepositAttributedToCurrentOperator()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();
        _cashSessionRepository.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        var result = await _handler.HandleAsync(new CreateCashDepositCommand(session.Id, 50m, "Reforço de troco"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deposit = session.Movements.ShouldHaveSingleItem();
        deposit.EntryType.ShouldBe(CashSessionEntryType.Deposit);
        deposit.Amount.ShouldBe(50m);
        deposit.Reason.ShouldBe("Reforço de troco");
        deposit.CreatedBy.ShouldBe(_operatorId);
        session.ExpectedCashAmount.ShouldBe(150m);
        result.Value.ShouldBe(new CreateCashDepositResponse(deposit.Id, 50m, "Maria Souza", deposit.CreatedAt));
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}