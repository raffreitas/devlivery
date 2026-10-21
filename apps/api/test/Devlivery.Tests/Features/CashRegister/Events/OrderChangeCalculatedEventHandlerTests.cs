using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
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
public sealed class OrderChangeCalculatedEventHandlerTests
{
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly OrderChangeCalculatedEventHandler _handler;

    public OrderChangeCalculatedEventHandlerTests()
    {
        _handler = new OrderChangeCalculatedEventHandler(
            NullLogger<OrderChangeCalculatedEventHandler>.Instance,
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId),
            TestMocks.TenantAccessor(),
            _orderRepository);
    }

    [Fact]
    public async Task GivenCashPaidOrderAndActiveSession_WhenChangeCalculated_ThenRecordsChangeForCurrentOperator()
    {
        // Arrange
        var order = GivenOrderPaidWith(new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash));
        var session = GivenActiveSession(openingAmount: 100m);

        // Act
        await _handler.HandleAsync(CreateEvent(order.Id, 30m), CancellationToken.None);

        // Assert
        var change = session.Movements.ShouldHaveSingleItem();
        change.EntryType.ShouldBe(CashSessionEntryType.Change);
        change.RelatedOrderId.ShouldBe(order.Id);
        change.Amount.ShouldBe(30m);
        change.PaymentMethod.ShouldBe(PaymentMethod.Cash);
        change.CreatedBy.ShouldBe(_operatorId);
        session.ExpectedCashAmount.ShouldBe(70m);
    }

    [Fact]
    public async Task GivenChangeAlreadyRecorded_WhenSameEventIsHandledAgain_ThenDoesNotDuplicateIt()
    {
        // Arrange
        var order = GivenOrderPaidWith(new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash));
        var session = GivenActiveSession();
        await _handler.HandleAsync(CreateEvent(order.Id, 30m), CancellationToken.None);

        // Act
        await _handler.HandleAsync(CreateEvent(order.Id, 30m), CancellationToken.None);

        // Assert
        session.Movements.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GivenNonPositiveChange_WhenChangeCalculated_ThenRecordsNothing(decimal changeAmount)
    {
        // Arrange
        var order = GivenOrderPaidWith(new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash));
        var session = GivenActiveSession();

        // Act
        await _handler.HandleAsync(CreateEvent(order.Id, changeAmount), CancellationToken.None);

        // Assert
        session.Movements.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenOrderWithoutActiveCashPayment_WhenChangeCalculated_ThenRecordsNothing()
    {
        // Arrange
        var order = GivenOrderPaidWith(
            new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).AsCancelled(),
            new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Pix));
        var session = GivenActiveSession();

        // Act
        await _handler.HandleAsync(CreateEvent(order.Id, 30m), CancellationToken.None);

        // Assert
        session.Movements.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenUnknownOrder_WhenChangeCalculated_ThenRecordsNothing()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var session = GivenActiveSession();

        // Act
        await _handler.HandleAsync(CreateEvent(Guid.NewGuid(), 30m), CancellationToken.None);

        // Assert
        session.Movements.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenNoActiveSession_WhenChangeCalculated_ThenAllowsDeliveryOutsideCashSession()
    {
        // Arrange
        var order = GivenOrderPaidWith(new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash));
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _handler.HandleAsync(CreateEvent(order.Id, 30m), CancellationToken.None));

        // Assert
        exception.ShouldBeNull();
    }

    private Order GivenOrderPaidWith(params OrderPaymentBuilder[] payments)
    {
        var order = new OrderBuilder().WithCustomPayments(payments.Select(p => p.Build())).Build();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        return order;
    }

    private CashSession GivenActiveSession(decimal openingAmount = 0m)
    {
        var session = new CashSessionBuilder().WithOpeningAmount(openingAmount).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        return session;
    }

    private static OrderChangeCalculatedEvent CreateEvent(Guid orderId, decimal change)
    {
        return new OrderChangeCalculatedEvent(orderId, Guid.NewGuid(), change, DateTime.UtcNow);
    }
}
