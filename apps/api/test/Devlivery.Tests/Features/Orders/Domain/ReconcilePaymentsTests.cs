using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class ReconcilePaymentsTests
{
    [Fact]
    public void GivenPaymentWithoutId_WhenReconciling_ThenAddsNewPendingPayment()
    {
        // Arrange
        var existing = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(30m).Build();
        var order = CreateOrder(existing);

        // Act
        order.ReconcilePayments([
            new OrderPaymentUpdate(existing.Id, PaymentMethod.Cash, 30m),
            new OrderPaymentUpdate(null, PaymentMethod.Pix, 20m)
        ]);

        // Assert
        order.Payments.Count.ShouldBe(2);
        var added = order.Payments.Single(p => p.Id != existing.Id);
        added.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        added.Amount.ShouldBe(20m);
        added.PaymentStatus.ShouldBe(PaymentStatus.Pending);
        added.EstablishmentId.ShouldBe(order.EstablishmentId);
    }

    [Fact]
    public void GivenChangedPendingPayment_WhenReconciling_ThenUpdatesMethodAndAmount()
    {
        // Arrange
        var existing = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(30m).Build();
        var order = CreateOrder(existing);

        // Act
        order.ReconcilePayments([new OrderPaymentUpdate(existing.Id, PaymentMethod.Pix, 45m)]);

        // Assert
        existing.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        existing.Amount.ShouldBe(45m);
        existing.PaymentStatus.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void GivenPendingPaymentMissingFromUpdates_WhenReconciling_ThenCancelsIt()
    {
        // Arrange
        var kept = new OrderPaymentBuilder().Build();
        var removed = new OrderPaymentBuilder().Build();
        var order = CreateOrder(kept, removed);

        // Act
        order.ReconcilePayments([new OrderPaymentUpdate(kept.Id, kept.PaymentMethod, kept.Amount)]);

        // Assert
        removed.PaymentStatus.ShouldBe(PaymentStatus.Cancelled);
        kept.PaymentStatus.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void GivenUnchangedConfirmedPayment_WhenReconciling_ThenKeepsItConfirmed()
    {
        // Arrange
        var confirmed = new OrderPaymentBuilder().AsConfirmed().Build();
        var order = CreateOrder(confirmed);

        // Act
        var exception = Record.Exception(() => order.ReconcilePayments([
            new OrderPaymentUpdate(confirmed.Id, confirmed.PaymentMethod, confirmed.Amount)
        ]));

        // Assert
        exception.ShouldBeNull();
        confirmed.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
    }

    [Fact]
    public void GivenChangedConfirmedPayment_WhenReconciling_ThenThrowsAskingForRefund()
    {
        // Arrange
        var confirmed = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(30m)
            .AsConfirmed().Build();
        var order = CreateOrder(confirmed);

        // Act
        var exception = Record.Exception(() => order.ReconcilePayments([
            new OrderPaymentUpdate(confirmed.Id, PaymentMethod.Pix, 30m)
        ]));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldContain("estorno");
        confirmed.PaymentMethod.ShouldBe(PaymentMethod.Cash);
    }

    [Fact]
    public void GivenConfirmedPaymentMissingFromUpdates_WhenReconciling_ThenThrowsAskingForRefund()
    {
        // Arrange
        var confirmed = new OrderPaymentBuilder().AsConfirmed().Build();
        var order = CreateOrder(confirmed);

        // Act
        var exception = Record.Exception(() => order.ReconcilePayments([
            new OrderPaymentUpdate(null, PaymentMethod.Pix, 10m)
        ]));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldContain("estorno");
        confirmed.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
    }

    [Fact]
    public void GivenDeliveredOrder_WhenReconcilingWithNewPayment_ThenThrowsDomainException()
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(OrderStatus.Delivered).Build();
        var updates = order.Payments
            .Select(p => new OrderPaymentUpdate(p.Id, p.PaymentMethod, p.Amount))
            .Append(new OrderPaymentUpdate(null, PaymentMethod.Pix, 10m))
            .ToList();

        // Act
        var exception = Record.Exception(() => order.ReconcilePayments(updates));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }

    private static Order CreateOrder(params OrderPayment[] payments)
    {
        return new OrderBuilder().WithCustomPayments(payments).Build();
    }
}