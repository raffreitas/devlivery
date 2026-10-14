using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CloseCashSessionHandlerTests
{
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CloseCashSessionHandler _handler;

    public CloseCashSessionHandlerTests()
    {
        _handler = new CloseCashSessionHandler(_cashSessionRepository, _unitOfWork);
    }

    [Fact]
    public async Task GivenNonexistentSession_WhenClosing_ThenReturnsNotFoundError()
    {
        // Arrange
        _cashSessionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _handler.HandleAsync(new CloseCashSessionCommand(Guid.NewGuid(), 100m, null),
            CancellationToken.None);

        // Assert
        result.HasError<NotFoundError>().ShouldBeTrue();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenClosedSession_WhenClosingAgain_ThenReturnsValidationErrorKeepingOriginalClosing()
    {
        // Arrange
        var session = new CashSessionBuilder().AsClosed(100m).Build();
        _cashSessionRepository.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        var result = await _handler.HandleAsync(new CloseCashSessionCommand(session.Id, 200m, null),
            CancellationToken.None);

        // Assert
        result.HasError<ValidationError>().ShouldBeTrue();
        session.ClosingAmount.ShouldBe(100m);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenOpenSession_WhenClosing_ThenClosesWithInformedAmountAndCommits()
    {
        // Arrange
        var session = new CashSessionBuilder().Build();
        _cashSessionRepository.GetByIdAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        // Act
        var result = await _handler.HandleAsync(new CloseCashSessionCommand(session.Id, 180m, "Conferido"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        session.Status.ShouldBe(CashSessionStatus.Closed);
        session.ClosingAmount.ShouldBe(180m);
        session.Notes.ShouldBe("Conferido");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}