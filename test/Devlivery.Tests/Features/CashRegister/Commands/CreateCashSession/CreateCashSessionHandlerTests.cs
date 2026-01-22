using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CreateCashSessionHandlerTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Create_CashSession_When_No_Active_Session_Exists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(tenantId);
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "João Silva",
            OpeningAmount: 100.00m,
            Notes: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.OpeningAmount.ShouldBe(100.00m);
        result.Value.AttendantName.ShouldBe("João Silva");
        result.Value.Status.ShouldBe("Open");
    }

    [Fact]
    public async Task Handle_Should_Call_AddAsync_On_Repository()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "Maria Santos",
            OpeningAmount: 50.00m,
            Notes: "Abertura");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await cashSessionRepository.Received(1)
            .AddAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Call_SaveChangesAsync_On_UnitOfWork()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "Pedro Costa",
            OpeningAmount: 200.00m,
            Notes: null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationError_When_Active_Session_Exists()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var activeSession = fixture.CreateCashSession();
        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(activeSession);

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "Ana Lima",
            OpeningAmount: 150.00m,
            Notes: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Not_Call_AddAsync_When_Active_Session_Exists()
    {
        // Arrange
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        var activeSession = fixture.CreateCashSession();
        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(activeSession);

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "Carlos Souza",
            OpeningAmount: 100.00m,
            Notes: null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await cashSessionRepository.DidNotReceive()
            .AddAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_TenantId_From_TenantAccessor()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var tenantAccessor = fixture.CreateTenantAccessorMock(expectedTenantId);
        var cashSessionRepository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();

        CashSession? capturedSession = null;
        cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        await cashSessionRepository.AddAsync(Arg.Do<CashSession>(s => capturedSession = s),
            Arg.Any<CancellationToken>());

        var handler = new CreateCashSessionHandler(
            cashSessionRepository,
            unitOfWork,
            tenantAccessor);

        var command = new CreateCashSessionCommand(
            AttendantId: Guid.NewGuid(),
            AttendantName: "Teste",
            OpeningAmount: 100.00m,
            Notes: null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSession.ShouldNotBeNull();
        capturedSession.EstablishmentId.ShouldBe(expectedTenantId);
    }
}