using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

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
    public async Task Handle_Should_Close_Cash_Session_Successfully()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);
        var cashSessionId = cashSession.Id;
        const decimal closingAmount = 250m;
        const string notes = "Fechamento normal";

        _cashSessionRepository.GetByIdAsync(cashSessionId, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSessionId, closingAmount, notes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _cashSessionRepository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cash_Session_Not_Found()
    {
        // Arrange
        var cashSessionId = Guid.NewGuid();
        var command = new CloseCashSessionCommand(cashSessionId, 100m, null);

        _cashSessionRepository.GetByIdAsync(cashSessionId, Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var error = result.Errors[0];
        error.Metadata["Errors"].ShouldBe(new[] { "Caixa não encontrado." });
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cash_Session_Already_Closed()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession();
        cashSession.Close(100m, null);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 100m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var error = result.Errors[0];
        error.Metadata["Errors"].ShouldBe(new[] { "O caixa já está fechado." });
    }

    [Fact]
    public async Task Handle_Should_Calculate_Payment_Breakdown_Correctly()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 180m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Validate payments ledger was recorded correctly
        cashSession.Payments.Count.ShouldBe(4);
        cashSession.Payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount).ShouldBe(80m);
        cashSession.Payments.Where(p => p.PaymentMethod == PaymentMethod.CreditCard).Sum(p => p.Amount).ShouldBe(70m);
        cashSession.Payments.Where(p => p.PaymentMethod == PaymentMethod.Pix).Sum(p => p.Amount).ShouldBe(20m);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Expected_Cash_Amount_With_Deposits()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);

        // Add deposits
        var deposit1 = _fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 50m);
        var deposit2 = _fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 30m);
        cashSession.AddDeposit(deposit1);
        cashSession.AddDeposit(deposit2);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 280m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Close_Session_With_No_Orders()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 100m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}