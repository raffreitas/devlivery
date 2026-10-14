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
public sealed class OrderDeletedEventHandlerTests
{
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly OrderDeletedEventHandler _handler;

    public OrderDeletedEventHandlerTests()
    {
        _handler = new OrderDeletedEventHandler(
            NullLogger<OrderDeletedEventHandler>.Instance,
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId),
            TestMocks.TenantAccessor());
    }

    [Fact]
    public async Task GivenDeletedOrderWithPayments_WhenHandled_ThenReversesEachOrderPayment()
    {
        // Arrange
        var cashPaymentId = Guid.NewGuid();
        var cardPaymentId = Guid.NewGuid();
        var session = new CashSessionBuilder()
            .WithOpeningAmount(100m)
            .WithPayment(40m, PaymentMethod.Cash, _orderId, cashPaymentId)
            .WithPayment(20m, PaymentMethod.CreditCard, _orderId, cardPaymentId)
            .WithPayment(15m, PaymentMethod.Cash)
            .Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.HandleAsync(CreateEvent(), CancellationToken.None);

        // Assert
        var refunds = session.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ToList();
        refunds.Select(r => r.OrderPaymentId).ShouldBe([cashPaymentId, cardPaymentId], ignoreOrder: true);
        refunds.ShouldAllBe(r => r.RelatedOrderId == _orderId && r.Reason == "Pedido Excluído"
                                                              && r.CreatedBy == _operatorId);
        session.TotalRevenue.ShouldBe(15m);
        session.ExpectedCashAmount.ShouldBe(115m);
    }

    [Fact]
    public async Task GivenAlreadyReversedOrder_WhenDeletedEventIsHandledAgain_ThenDoesNotDuplicateRefunds()
    {
        // Arrange
        var session = new CashSessionBuilder().WithPayment(40m, PaymentMethod.Cash, _orderId).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        await _handler.HandleAsync(CreateEvent(), CancellationToken.None);

        // Act
        await _handler.HandleAsync(CreateEvent(), CancellationToken.None);

        // Assert
        session.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GivenOrderWithoutPaymentsInSession_WhenDeleted_ThenRecordsNothing()
    {
        // Arrange
        var session = new CashSessionBuilder().WithPayment(40m, PaymentMethod.Cash).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        // Act
        await _handler.HandleAsync(CreateEvent(), CancellationToken.None);

        // Assert
        session.Movements.ShouldHaveSingleItem().EntryType.ShouldBe(CashSessionEntryType.Payment);
    }

    [Fact]
    public async Task GivenNoActiveSession_WhenDeleted_ThenSkipsWithoutFailing()
    {
        // Arrange
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var exception = await Record.ExceptionAsync(() => _handler.HandleAsync(CreateEvent(), CancellationToken.None));

        // Assert
        exception.ShouldBeNull();
    }

    private OrderDeletedEvent CreateEvent()
    {
        return new OrderDeletedEvent(_orderId, Guid.NewGuid(), 60m, OrderStatus.Delivered, DateTime.UtcNow);
    }
}