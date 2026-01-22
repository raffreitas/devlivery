using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CloseCashSessionHandlerTests : IClassFixture<CashRegisterUnitTestFixture>
{
    private readonly CashRegisterUnitTestFixture _fixture;
    private readonly ICashSessionRepository _cashSessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloseCashSessionHandler _handler;

    public CloseCashSessionHandlerTests(CashRegisterUnitTestFixture fixture)
    {
        _fixture = fixture;
        _cashSessionRepository = Substitute.For<ICashSessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CloseCashSessionHandler(_cashSessionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Close_Session_When_Open()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession();
        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 120.50m, "Fechamento");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cashSession.Status.ShouldBe(CashSessionStatus.Closed);
        cashSession.ClosingAmount.ShouldBe(120.50m);
    }

    [Fact]
    public async Task Handle_Should_Call_UpdateAsync_And_SaveChanges()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession();
        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 80m, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _cashSessionRepository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        _cashSessionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new CloseCashSessionCommand(Guid.NewGuid(), 50m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors[0].Message.ShouldContain("não encontrado", Case.Insensitive);
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationError_When_Session_Already_Closed()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession();
        cashSession.Close(100m, null);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 50m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }
}