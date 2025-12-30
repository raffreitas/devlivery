using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;
using Devlivery.Features.CashRegister.Domain;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashDeposit;

[Collection("CashRegister Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CreateCashDepositHandlerTests(CashRegisterUnitTestFixture fixture)
{
    [Fact]
    public async Task Handle_Should_Create_Deposit_When_Session_Is_Open()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(tenantId);
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var cashSession = fixture.CreateCashSession(establishmentId: tenantId);
        cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: cashSession.Id,
            AttendantId: Guid.NewGuid(),
            AttendantName: "João Silva",
            Amount: 50.00m,
            Notes: "Aporte");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(50.00m);
        result.Value.AttendantName.ShouldBe("João Silva");
    }

    [Fact]
    public async Task Handle_Should_Add_Deposit_To_CashSession()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var cashSession = fixture.CreateCashSession();
        cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: cashSession.Id,
            AttendantId: Guid.NewGuid(),
            AttendantName: "Maria Santos",
            Amount: 100.00m,
            Notes: null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        cashSession.Deposits.Count.ShouldBe(1);
        cashSession.Deposits.First().Amount.ShouldBe(100.00m);
    }

    [Fact]
    public async Task Handle_Should_Call_UpdateAsync_On_Repository()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var cashSession = fixture.CreateCashSession();
        cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: cashSession.Id,
            AttendantId: Guid.NewGuid(),
            AttendantName: "Pedro Costa",
            Amount: 75.00m,
            Notes: "Depósito");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await cashSessionRepository.Received(1)
            .UpdateAsync(cashSession, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var cashSession = fixture.CreateCashSession();
        cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: cashSession.Id,
            AttendantId: Guid.NewGuid(),
            AttendantName: "Ana Lima",
            Amount: 200.00m,
            Notes: null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_NotFoundError_When_Session_Does_Not_Exist()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        cashSessionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: Guid.NewGuid(),
            AttendantId: Guid.NewGuid(),
            AttendantName: "Carlos Souza",
            Amount: 100.00m,
            Notes: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors[0].Message.ShouldContain("não encontrado");
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationError_When_Session_Is_Closed()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var cashSession = fixture.CreateCashSession();
        cashSession.Close(100.00m, null);

        cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new CreateCashDepositHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashDepositCommand(
            CashSessionId: cashSession.Id,
            AttendantId: Guid.NewGuid(),
            AttendantName: "Teste",
            Amount: 50.00m,
            Notes: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }
}