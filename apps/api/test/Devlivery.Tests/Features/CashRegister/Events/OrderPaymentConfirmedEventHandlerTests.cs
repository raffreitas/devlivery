using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Events;

[Trait("Category", "Unit Tests")]
public sealed class OrderPaymentConfirmedEventHandlerTests
{
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly OrderPaymentConfirmedEventHandler _handler;

    public OrderPaymentConfirmedEventHandlerTests()
    {
        _handler = new OrderPaymentConfirmedEventHandler(
            NullLogger<OrderPaymentConfirmedEventHandler>.Instance,
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId),
            TestMocks.TenantAccessor());
    }

    [Fact]
    public async Task GivenActiveSession_WhenPaymentConfirmed_ThenRecordsPaymentAttributedToCurrentOperator()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        var @event = CreateEvent(PaymentMethod.Cash, 50m);

        // Act
        await _handler.HandleAsync(@event, CancellationToken.None);

        // Assert
        var movement = session.Movements.ShouldHaveSingleItem();
        movement.EntryType.ShouldBe(CashSessionEntryType.Payment);
        movement.OrderPaymentId.ShouldBe(@event.PaymentId);
        movement.RelatedOrderId.ShouldBe(@event.OrderId);
        movement.PaymentMethod.ShouldBe(PaymentMethod.Cash);
        movement.Amount.ShouldBe(50m);
        movement.CreatedBy.ShouldBe(_operatorId);
        session.ExpectedCashAmount.ShouldBe(150m);
    }

    [Fact]
    public async Task GivenPaymentAlreadyRecorded_WhenSameEventIsHandledAgain_ThenDoesNotDuplicateIt()
    {
        // Arrange
        var session = new CashSessionBuilder().Build();
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        var @event = CreateEvent(PaymentMethod.Pix, 50m);
        await _handler.HandleAsync(@event, CancellationToken.None);

        // Act
        await _handler.HandleAsync(@event, CancellationToken.None);

        // Assert
        session.Movements.ShouldHaveSingleItem();
        session.TotalRevenue.ShouldBe(50m);
    }

    [Fact]
    public async Task GivenNoActiveSession_WhenPaymentConfirmed_ThenThrowsToPreventUnrecordedPayment()
    {
        // Arrange
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _handler.HandleAsync(CreateEvent(PaymentMethod.Cash, 50m), CancellationToken.None));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }

    private static OrderPaymentConfirmedEvent CreateEvent(PaymentMethod method, decimal amount)
    {
        return new OrderPaymentConfirmedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), method, amount, amount);
    }
}
