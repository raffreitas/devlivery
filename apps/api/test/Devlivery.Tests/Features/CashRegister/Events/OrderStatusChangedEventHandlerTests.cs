using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderStatusChangedEventHandlerTests
{
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly OrderStatusChangedEventHandler _handler;

    public OrderStatusChangedEventHandlerTests()
    {
        _handler = new OrderStatusChangedEventHandler(
            NullLogger<OrderStatusChangedEventHandler>.Instance,
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId),
            TestMocks.TenantAccessor());
    }

    [Fact]
    public async Task GivenCanceledOrderWithPayments_WhenStatusChanged_ThenReversesEachOrderPayment()
    {
        // Arrange
        var cashPaymentId = Guid.NewGuid();
        var pixPaymentId = Guid.NewGuid();
        var session = new CashSessionBuilder()
            .WithOpeningAmount(100m)
            .WithPayment(40m, PaymentMethod.Cash, _orderId, cashPaymentId)
            .WithPayment(20m, PaymentMethod.Pix, _orderId, pixPaymentId)
            .WithPayment(15m, PaymentMethod.Cash)
            .Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.HandleAsync(CreateEvent(OrderStatus.Canceled), CancellationToken.None);

        // Assert
        var refunds = session.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        refunds.Select(r => r.OrderPaymentId).ShouldBe([cashPaymentId, pixPaymentId], ignoreOrder: true);
        refunds.ShouldAllBe(r => r.RelatedOrderId == _orderId && r.Reason == "Pedido Cancelado"
                                                              && r.CreatedBy == _operatorId);
        refunds.Single(r => r.OrderPaymentId == cashPaymentId).Amount.ShouldBe(40m);
        refunds.Single(r => r.OrderPaymentId == pixPaymentId).PaymentMethod.ShouldBe(PaymentMethod.Pix);
        session.TotalRevenue.ShouldBe(15m);
        session.ExpectedCashAmount.ShouldBe(115m);
    }

    [Fact]
    public async Task GivenAlreadyReversedOrder_WhenCanceledEventIsHandledAgain_ThenDoesNotDuplicateRefunds()
    {
        // Arrange
        var session = new CashSessionBuilder().WithPayment(40m, PaymentMethod.Cash, _orderId).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        await _handler.HandleAsync(CreateEvent(OrderStatus.Canceled), CancellationToken.None);

        // Act
        await _handler.HandleAsync(CreateEvent(OrderStatus.Canceled), CancellationToken.None);

        // Assert
        session.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Delivered)]
    public async Task GivenNonCanceledStatus_WhenStatusChanged_ThenDoesNotReversePayments(OrderStatus newStatus)
    {
        // Arrange
        var session = new CashSessionBuilder().WithPayment(40m, PaymentMethod.Cash, _orderId).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.HandleAsync(CreateEvent(newStatus), CancellationToken.None);

        // Assert
        session.Movements.ShouldNotContain(m => m.EntryType == CashSessionEntryType.Refund);
    }

    [Fact]
    public async Task GivenOrderWithoutPaymentsInSession_WhenCanceled_ThenRecordsNothing()
    {
        // Arrange
        var session = new CashSessionBuilder().WithPayment(40m, PaymentMethod.Cash).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.HandleAsync(CreateEvent(OrderStatus.Canceled), CancellationToken.None);

        // Assert
        session.Movements.ShouldHaveSingleItem().EntryType.ShouldBe(CashSessionEntryType.Payment);
    }

    [Fact]
    public async Task GivenNoActiveSession_WhenCanceled_ThenSkipsWithoutFailing()
    {
        // Arrange
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _handler.HandleAsync(CreateEvent(OrderStatus.Canceled), CancellationToken.None));

        // Assert
        exception.ShouldBeNull();
    }

    private OrderStatusChangedEvent CreateEvent(OrderStatus newStatus)
    {
        return new OrderStatusChangedEvent(_orderId, Guid.NewGuid(), OrderStatus.Pending, newStatus, 60m,
            DateTime.UtcNow);
    }
}