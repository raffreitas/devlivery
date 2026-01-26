using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderDeletedEventHandlerTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Add_Reversal_For_Single_Payment()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
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

        var @event = new OrderDeletedEvent(
            OrderId: orderId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            Total: 50m,
            Status: OrderStatus.Delivered,
            DeletedAt: DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(1);
        reversals[0].Amount.ShouldBe(50m);
        reversals[0].OrderPaymentId.ShouldBe(paymentId);
        reversals[0].Reason.ShouldBe("Pedido Excluído");
        
        cashSession.TotalRevenue.ShouldBe(0m); // Payment - Reversal = 0

        await repository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Add_Reversals_For_Multi_Payment_Order()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
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

        var @event = new OrderDeletedEvent(
            OrderId: orderId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            Total: 60m,
            Status: OrderStatus.Delivered,
            DeletedAt: DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(3);
        reversals.Sum(r => r.Amount).ShouldBe(60m);
        
        cashSession.TotalRevenue.ShouldBe(0m); // All payments reversed
        cashSession.ExpectedCashAmount.ShouldBe(100m); // Back to opening amount

        await repository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Be_Idempotent_For_Same_Order()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
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

        var @event = new OrderDeletedEvent(
            OrderId: orderId,
            EstablishmentId: tenantAccessor.Tenant.Id,
            Total: 50m,
            Status: OrderStatus.Delivered,
            DeletedAt: DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None); // First time
        await handler.Handle(@event, CancellationToken.None); // Duplicate

        // Assert - only one reversal recorded
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(1);
        
        await repository.Received(2).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Add_Reversal_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        var @event = new OrderDeletedEvent(
            OrderId: Guid.NewGuid(),
            EstablishmentId: tenantAccessor.Tenant.Id,
            Total: 50m,
            Status: OrderStatus.Delivered,
            DeletedAt: DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

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
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
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

        var @event = new OrderDeletedEvent(
            OrderId: Guid.NewGuid(), // Order not in session
            EstablishmentId: tenantAccessor.Tenant.Id,
            Total: 50m,
            Status: OrderStatus.Delivered,
            DeletedAt: DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var reversals = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        reversals.Count.ShouldBe(0);
        
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Log_Warning_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderDeletedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        var @event = new OrderDeletedEvent(
            Guid.NewGuid(), tenantAccessor.Tenant.Id, 50m, OrderStatus.Delivered, DateTime.UtcNow
        );

        var handler = new OrderDeletedEventHandler(logger, repository, unitOfWork, tenantAccessor);

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
