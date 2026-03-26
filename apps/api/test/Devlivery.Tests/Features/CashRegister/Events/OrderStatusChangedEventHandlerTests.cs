using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderStatusChangedEventHandlerTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Add_Reversal_When_Status_Changes_To_Canceled()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(50m, PaymentMethod.Cash, orderId, paymentId)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderStatusChangedEvent(
            OrderId: orderId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            OldStatus: OrderStatus.Delivered,
            NewStatus: OrderStatus.Canceled,
            TotalAmount: 50m,
            ChangedAt: DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(1);
        reversals[0].Amount.ShouldBe(50m);
        reversals[0].Reason.ShouldBe("Pedido Cancelado");

        cashSession.TotalRevenue.ShouldBe(0m);

        await repository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Process_When_Status_Is_Not_Canceled()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(50m, PaymentMethod.Cash, orderId, paymentId)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act - Test various non-canceled status transitions
        await handler.Handle(new OrderStatusChangedEvent(
            orderId, tenantAccessor.Tenant.Id, OrderStatus.Pending, OrderStatus.Preparing, 50m, DateTime.UtcNow
        ), CancellationToken.None);

        await handler.Handle(new OrderStatusChangedEvent(
            orderId, tenantAccessor.Tenant.Id, OrderStatus.Preparing, OrderStatus.Ready, 50m, DateTime.UtcNow
        ), CancellationToken.None);

        await handler.Handle(new OrderStatusChangedEvent(
            orderId, tenantAccessor.Tenant.Id, OrderStatus.Ready, OrderStatus.Delivered, 50m, DateTime.UtcNow
        ), CancellationToken.None);

        // Assert - no reversals added
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(0);

        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Add_Reversals_For_Multi_Payment_Order_When_Canceled()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var payment1Id = Guid.NewGuid();
        var payment2Id = Guid.NewGuid();
        var payment3Id = Guid.NewGuid();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(30m, PaymentMethod.Cash, orderId, payment1Id)
            .WithPayment(20m, PaymentMethod.CreditCard, orderId, payment2Id)
            .WithPayment(10m, PaymentMethod.Pix, orderId, payment3Id)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderStatusChangedEvent(
            OrderId: orderId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            OldStatus: OrderStatus.Delivered,
            NewStatus: OrderStatus.Canceled,
            TotalAmount: 60m,
            ChangedAt: DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(3);
        reversals.Sum(r => r.Amount).ShouldBe(60m);
        reversals.All(r => r.Reason == "Pedido Cancelado").ShouldBeTrue();

        cashSession.TotalRevenue.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_Should_Be_Idempotent_For_Same_Canceled_Order()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(50m, PaymentMethod.Cash, orderId, paymentId)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderStatusChangedEvent(
            orderId, tenantAccessor.Tenant.Id, OrderStatus.Delivered, OrderStatus.Canceled, 50m, DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None); // First time
        await handler.Handle(@event, CancellationToken.None); // Duplicate

        // Assert - only one reversal recorded
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Should_Not_Add_Reversal_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        var @event = new OrderStatusChangedEvent(
            Guid.NewGuid(), tenantAccessor.Tenant.Id, OrderStatus.Delivered, OrderStatus.Canceled, 50m, DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Add_Reversal_When_No_Matching_Payments()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(50m, PaymentMethod.Cash, Guid.NewGuid(), Guid.NewGuid()) // Different order
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderStatusChangedEvent(
            Guid.NewGuid(), tenantAccessor.Tenant.Id, OrderStatus.Delivered, OrderStatus.Canceled, 50m, DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_Should_Log_Information_When_Processing_Canceled_Status()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderStatusChangedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        var orderId = Guid.NewGuid();
        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .WithPayment(50m, PaymentMethod.Cash, orderId, Guid.NewGuid())
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderStatusChangedEvent(
            orderId, tenantAccessor.Tenant.Id, OrderStatus.Delivered, OrderStatus.Canceled, 50m, DateTime.UtcNow
        );

        var handler = new OrderStatusChangedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Canceled")),
            null,
            Arg.Any<Func<object, Exception?, string>>()!);
    }
}