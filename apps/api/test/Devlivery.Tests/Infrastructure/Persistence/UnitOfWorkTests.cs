using Devlivery.Common.Events;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Establishments;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Infrastructure.Identity.Authentication;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Persistence.Repositories;
using Devlivery.Infrastructure.Tenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Infrastructure.Persistence;

[Trait("Category", "Integration Tests")]
public sealed class UnitOfWorkTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    private readonly Establishment _establishment = new("Loja Teste", isActive: true);
    private readonly Guid _actorId = Guid.NewGuid();
    private readonly SaveChangesCounter _saveCounter = new();

    private Guid TenantId => _establishment.Id;

    [Fact]
    public async Task GivenDeliveredOrderPaidInCash_WhenCommitting_ThenPersistsOrderAndCashMovementsInSingleSave()
    {
        // Arrange
        await using var provider = BuildServiceProvider(services => services.AddCashRegisterEventHandlers());
        var (orderId, sessionId) = await SeedOrderAndOpenCashSessionAsync(provider, paidInCash: 60m, orderTotal: 50m);

        // Act
        await using (var scope = provider.CreateAsyncScope())
        {
            var order = await scope.ServiceProvider.GetRequiredService<IOrderRepository>()
                .GetByIdAsync(orderId);
            order!.UpdateStatus(OrderStatus.Delivered);

            _saveCounter.Reset();
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        // Assert
        _saveCounter.Count.ShouldBe(1);

        await using var assertScope = provider.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var persistedOrder = await dbContext.Orders.Include(o => o.Payments).SingleAsync(o => o.Id == orderId);
        persistedOrder.Status.ShouldBe(OrderStatus.Delivered);
        persistedOrder.Payments.ShouldAllBe(p => p.PaymentStatus == PaymentStatus.Confirmed);

        var movements = await dbContext.CashSessionMovements
            .Where(m => m.CashSessionId == sessionId)
            .ToListAsync();
        movements.Count.ShouldBe(2);
        movements.ShouldContain(m => m.EntryType == CashSessionEntryType.Payment && m.Amount == 60m);
        movements.ShouldContain(m => m.EntryType == CashSessionEntryType.Change && m.Amount == 10m);
        movements.ShouldAllBe(m => m.CreatedBy == _actorId);
    }

    [Fact]
    public async Task GivenFailingEventHandler_WhenCommitting_ThenPersistsNothing()
    {
        // Arrange
        await using var provider = BuildServiceProvider(services =>
        {
            services.AddCashRegisterEventHandlers();
            services.AddScoped<IDomainEventHandler<OrderStatusChangedEvent>, ThrowingHandler>();
        });
        var (orderId, sessionId) = await SeedOrderAndOpenCashSessionAsync(provider, paidInCash: 50m, orderTotal: 50m);

        // Act
        Exception? exception;
        await using (var scope = provider.CreateAsyncScope())
        {
            var order = await scope.ServiceProvider.GetRequiredService<IOrderRepository>()
                .GetByIdAsync(orderId);
            order!.UpdateStatus(OrderStatus.Delivered);

            _saveCounter.Reset();
            exception = await Record.ExceptionAsync(() =>
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync());
        }

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>();
        _saveCounter.Count.ShouldBe(0);

        await using var assertScope = provider.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (await dbContext.Orders.SingleAsync(o => o.Id == orderId)).Status.ShouldBe(OrderStatus.Pending);
        (await dbContext.CashSessionMovements.AnyAsync(m => m.CashSessionId == sessionId)).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenHandlerRaisingNewEvent_WhenCommitting_ThenDispatchesItAndClearsEvents()
    {
        // Arrange
        var recorder = new StatusChangeRecorder();
        await using var provider = BuildServiceProvider(services =>
        {
            services.AddSingleton(recorder);
            services.AddScoped<IDomainEventHandler<OrderStatusChangedEvent>, AdvanceToReadyHandler>();
        });
        var (orderId, _) = await SeedOrderAndOpenCashSessionAsync(provider, paidInCash: 50m, orderTotal: 50m);

        // Act
        Order order;
        await using (var scope = provider.CreateAsyncScope())
        {
            order = (await scope.ServiceProvider.GetRequiredService<IOrderRepository>().GetByIdAsync(orderId))!;
            order.UpdateStatus(OrderStatus.Preparing);

            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        // Assert
        recorder.Changes.ShouldBe([
            (OrderStatus.Pending, OrderStatus.Preparing),
            (OrderStatus.Preparing, OrderStatus.Ready)
        ]);
        order.DomainEvents.ShouldBeEmpty();

        await using var assertScope = provider.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbContext.Orders.SingleAsync(o => o.Id == orderId)).Status.ShouldBe(OrderStatus.Ready);
    }

    [Fact]
    public async Task GivenHandlersRaisingEventsEndlessly_WhenCommitting_ThenThrowsWithoutSaving()
    {
        // Arrange
        await using var provider = BuildServiceProvider(services =>
            services.AddScoped<IDomainEventHandler<OrderStatusChangedEvent>, PingPongHandler>());
        var (orderId, _) = await SeedOrderAndOpenCashSessionAsync(provider, paidInCash: 50m, orderTotal: 50m);

        await using var scope = provider.CreateAsyncScope();
        var order = await scope.ServiceProvider.GetRequiredService<IOrderRepository>().GetByIdAsync(orderId);
        order!.UpdateStatus(OrderStatus.Preparing);

        _saveCounter.Reset();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync());

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>();
        _saveCounter.Count.ShouldBe(0);
    }

    private ServiceProvider BuildServiceProvider(Action<IServiceCollection> configureHandlers)
    {
        var tenantAccessor = new TenantAccessor();
        tenantAccessor.Register(new Tenant(TenantId));

        var currentUserAccessor = Substitute.For<ICurrentUserAccessor>();
        currentUserAccessor.ResolveAsync(Arg.Any<CancellationToken>())
            .Returns(new ResolvedCurrentUser(_actorId, "Operador"));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantAccessor>(tenantAccessor);
        services.AddSingleton(currentUserAccessor);
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(database.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(_saveCounter));
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICashSessionRepository, CashSessionRepository>();
        configureHandlers(services);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private async Task<(Guid OrderId, Guid SessionId)> SeedOrderAndOpenCashSessionAsync(
        IServiceProvider provider,
        decimal paidInCash,
        decimal orderTotal)
    {
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var product = new Product("Pizza", "Pizza grande", orderTotal, "Pizzas", available: true, TenantId);
        var order = new Order(
            customer: CustomerInfo.Create("Maria"),
            deliveryAddress: new DeliveryAddress("Rua A, 123", null),
            deliveryFee: 0m,
            establishmentId: TenantId,
            items: [new OrderItem(product.Id, TenantId, quantity: 1, unitPrice: orderTotal, notes: null)],
            payments: [new OrderPayment(TenantId, PaymentMethod.Cash, paidInCash)]);
        var session = new CashSession(TenantId, _actorId, "Operador", openingAmount: 100m, notes: null);

        dbContext.AddRange(_establishment, product, order, session);
        await dbContext.SaveChangesAsync();

        return (order.Id, session.Id);
    }

    private sealed class SaveChangesCounter : SaveChangesInterceptor
    {
        public int Count { get; private set; }

        public void Reset() => Count = 0;

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            Count++;
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class ThrowingHandler : IDomainEventHandler<OrderStatusChangedEvent>
    {
        public Task HandleAsync(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Falha simulada no handler.");
    }

    private sealed class StatusChangeRecorder
    {
        public List<(OrderStatus Old, OrderStatus New)> Changes { get; } = [];
    }

    private sealed class AdvanceToReadyHandler(StatusChangeRecorder recorder, IOrderRepository orderRepository)
        : IDomainEventHandler<OrderStatusChangedEvent>
    {
        public async Task HandleAsync(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            recorder.Changes.Add((domainEvent.OldStatus, domainEvent.NewStatus));

            if (domainEvent.NewStatus != OrderStatus.Preparing)
                return;

            var order = await orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
            order!.UpdateStatus(OrderStatus.Ready);
        }
    }

    private sealed class PingPongHandler(IOrderRepository orderRepository)
        : IDomainEventHandler<OrderStatusChangedEvent>
    {
        public async Task HandleAsync(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(domainEvent.OrderId, cancellationToken);
            order!.UpdateStatus(domainEvent.NewStatus == OrderStatus.Preparing
                ? OrderStatus.Ready
                : OrderStatus.Preparing);
        }
    }
}

internal static class CashRegisterEventHandlersRegistration
{
    public static IServiceCollection AddCashRegisterEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<OrderPaymentConfirmedEvent>, OrderPaymentConfirmedEventHandler>();
        services.AddScoped<IDomainEventHandler<OrderChangeCalculatedEvent>, OrderChangeCalculatedEventHandler>();
        services.AddScoped<IDomainEventHandler<OrderStatusChangedEvent>, OrderStatusChangedEventHandler>();
        services.AddScoped<IDomainEventHandler<OrderDeletedEvent>, OrderDeletedEventHandler>();
        return services;
    }
}