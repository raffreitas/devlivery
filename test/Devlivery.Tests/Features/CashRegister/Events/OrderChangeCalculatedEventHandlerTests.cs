using Devlivery.Common.Domain.Enums;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Events;
using Devlivery.Tests.Common.Builders;

using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderChangeCalculatedEventHandlerTests(CashRegisterUnitTestFixture fixture) : IClassFixture<CashRegisterUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Record_Change_When_Cash_Payment_And_ActiveSession()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        var order = new OrderBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithPaymentMethod(PaymentMethod.Cash, 20m)
            .Build();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderChangeCalculatedEvent(order.Id, tenantAccessor.Tenant.Id, 5m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var changes = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Change).ToList();
        changes.Count.ShouldBe(1);
        changes[0].Amount.ShouldBe(5m);
        changes[0].PaymentMethod.ShouldBe(PaymentMethod.Cash);

        await repository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Record_When_Change_Is_LessOrEqual_Zero()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        var @event = new OrderChangeCalculatedEvent(Guid.NewGuid(), tenantAccessor.Tenant.Id, 0m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - nothing recorded
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Record_When_Order_Not_Found()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var @event = new OrderChangeCalculatedEvent(Guid.NewGuid(), tenantAccessor.Tenant.Id, 5m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Record_When_Order_Has_No_Cash_Payment()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        var order = new OrderBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithPaymentMethod(PaymentMethod.CreditCard, 50m)
            .Build();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderChangeCalculatedEvent(order.Id, tenantAccessor.Tenant.Id, 5m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - no change recorded
        var changes = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Change).ToList();
        changes.Count.ShouldBe(0);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Record_When_No_Active_Session()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        var order = new OrderBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithPaymentMethod(PaymentMethod.Cash, 30m)
            .Build();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .ReturnsNull();

        var @event = new OrderChangeCalculatedEvent(order.Id, tenantAccessor.Tenant.Id, 5m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - no update when no active session
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Be_Idempotent_When_Change_Already_Exists()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OrderChangeCalculatedEventHandler>>();
        var repository = fixture.CreateCashSessionRepositoryMock();
        var unitOfWork = fixture.CreateUnitOfWorkMock();
        var tenantAccessor = fixture.CreateTenantAccessorMock();
        var orderRepository = Substitute.For<IOrderRepository>();

        var order = new OrderBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithPaymentMethod(PaymentMethod.Cash, 40m)
            .Build();

        orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var cashSession = new CashSessionBuilder()
            .WithEstablishmentId(tenantAccessor.Tenant.Id)
            .WithOpeningAmount(100m)
            .Build();

        // Pre-add change to simulate existing entry
        cashSession.AddChange(order.Id, 5m, PaymentMethod.Cash);

        repository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var @event = new OrderChangeCalculatedEvent(order.Id, tenantAccessor.Tenant.Id, 5m, DateTime.UtcNow);

        var handler = new OrderChangeCalculatedEventHandler(logger, repository, unitOfWork, tenantAccessor, orderRepository);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - still only one change
        var changes = cashSession.Movements.Where(m => m.EntryType == CashSessionEntryType.Change).ToList();
        changes.Count.ShouldBe(1);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
    }
}
