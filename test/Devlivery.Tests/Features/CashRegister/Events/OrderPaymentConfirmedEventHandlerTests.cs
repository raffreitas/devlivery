using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Shared.Domain.Enums;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderPaymentConfirmedEventHandlerTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Add_Payment_To_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderPaymentConfirmedEvent(
            OrderId: Guid.NewGuid(),
            PaymentId: Guid.NewGuid(),
            EstablishmentId: tenantAccessor.Tenant.Id,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 50m,
            OrderTotal: 50m
        );

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        cashSession.Movements.Count.ShouldBe(1);
        cashSession.Movements.First().EntryType.ShouldBe(CashSessionEntryType.Payment);
        cashSession.Movements.First().Amount.ShouldBe(50m);
        cashSession.Movements.First().PaymentMethod.ShouldBe(PaymentMethod.Cash);
        cashSession.Movements.First().OrderPaymentId.ShouldBe(@event.PaymentId);
        cashSession.TotalRevenue.ShouldBe(50m);
        cashSession.ExpectedCashAmount.ShouldBe(150m); // 100 + 50

        await repository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Be_Idempotent_For_Same_PaymentId()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var paymentId = Guid.NewGuid();
        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderPaymentConfirmedEvent(
            OrderId: Guid.NewGuid(),
            PaymentId: paymentId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            PaymentMethod: PaymentMethod.CreditCard,
            Amount: 75m,
            OrderTotal: 75m
        );

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None); // First time
        await handler.Handle(@event, CancellationToken.None); // Duplicate

        // Assert - only one payment recorded
        cashSession.Movements.Count.ShouldBe(1);
        cashSession.TotalRevenue.ShouldBe(75m);
        await repository.Received(2).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Record_Multiple_Payments_From_Same_Order()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        var events = new[]
        {
            new OrderPaymentConfirmedEvent(orderId, Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.Cash, 30m, 60m),
            new OrderPaymentConfirmedEvent(orderId, Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.CreditCard, 20m, 60m),
            new OrderPaymentConfirmedEvent(orderId, Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.Pix, 10m, 60m)
        };

        // Act
        foreach (var @event in events)
        {
            await handler.Handle(@event, CancellationToken.None);
        }

        // Assert
        cashSession.Movements.Count.ShouldBe(3);
        cashSession.TotalRevenue.ShouldBe(60m);
        cashSession.TotalOrders.ShouldBe(3); // 3 different payment IDs
        
        var cashPayments = cashSession.Movements.Where(m => m.PaymentMethod == PaymentMethod.Cash).Sum(m => m.Amount);
        cashPayments.ShouldBe(30m);
        
        cashSession.ExpectedCashAmount.ShouldBe(130m); // 100 + 30 (only cash)
    }

    [Fact]
    public async Task Handle_Should_Not_Record_Payment_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        var @event = new OrderPaymentConfirmedEvent(
            OrderId: Guid.NewGuid(),
            PaymentId: Guid.NewGuid(),
            EstablishmentId: tenantAccessor.Tenant.Id,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 50m,
            OrderTotal: 50m
        );

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - no update calls
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Update_Expected_Cash_Amount_Only_For_Cash_Payments()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(new OrderPaymentConfirmedEvent(
            Guid.NewGuid(), Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.Cash, 50m, 50m),
            CancellationToken.None);
        
        await handler.Handle(new OrderPaymentConfirmedEvent(
            Guid.NewGuid(), Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.CreditCard, 30m, 30m),
            CancellationToken.None);

        // Assert
        cashSession.TotalRevenue.ShouldBe(80m); // 50 + 30
        cashSession.ExpectedCashAmount.ShouldBe(150m); // 100 + 50 (only cash payment)
    }

    [Fact]
    public async Task Handle_Should_Log_Information_When_Processing_Event()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderPaymentConfirmedEvent(
            Guid.NewGuid(), Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.Cash, 50m, 50m
        );

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>()!);
    }

    [Fact]
    public async Task Handle_Should_Log_Warning_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderPaymentConfirmedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        var @event = new OrderPaymentConfirmedEvent(
            Guid.NewGuid(), Guid.NewGuid(), tenantAccessor.Tenant.Id, PaymentMethod.Cash, 50m, 50m
        );

        var handler = new OrderPaymentConfirmedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No active cash session found")),
            null,
            Arg.Any<Func<object, Exception?, string>>()!);
    }
}
