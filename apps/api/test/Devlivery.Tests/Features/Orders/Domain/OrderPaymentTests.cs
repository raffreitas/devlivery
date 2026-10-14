using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class OrderPaymentTests
{
    [Fact]
    public void GivenPendingPayment_WhenConfirming_ThenMarksAsConfirmed()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().Build();

        // Act
        payment.Confirm();

        // Assert
        payment.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        payment.ConfirmedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(PaymentStatus.Confirmed)]
    [InlineData(PaymentStatus.Cancelled)]
    public void GivenNonPendingPayment_WhenConfirming_ThenThrowsDomainException(PaymentStatus status)
    {
        // Arrange
        var payment = BuildPayment(status);

        // Act
        var exception = Record.Exception(payment.Confirm);

        // Assert
        exception.ShouldBeOfType<DomainException>();
        payment.PaymentStatus.ShouldBe(status);
    }

    [Fact]
    public void GivenPendingPayment_WhenCancelling_ThenMarksAsCancelled()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().Build();

        // Act
        payment.Cancel();

        // Assert
        payment.PaymentStatus.ShouldBe(PaymentStatus.Cancelled);
    }

    [Fact]
    public void GivenPendingPayment_WhenUpdating_ThenChangesMethodAndAmount()
    {
        // Arrange
        var payment = new OrderPaymentBuilder().WithPaymentMethod(PaymentMethod.Cash).WithAmount(10m).Build();

        // Act
        payment.Update(PaymentMethod.Pix, 15m);

        // Assert
        payment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        payment.Amount.ShouldBe(15m);
    }

    [Theory]
    [InlineData(PaymentStatus.Confirmed)]
    [InlineData(PaymentStatus.Cancelled)]
    public void GivenNonPendingPayment_WhenUpdating_ThenThrowsDomainException(PaymentStatus status)
    {
        // Arrange
        var payment = BuildPayment(status);

        // Act
        var exception = Record.Exception(() => payment.Update(PaymentMethod.Pix, 15m));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }

    private static OrderPayment BuildPayment(PaymentStatus status)
    {
        var builder = new OrderPaymentBuilder();
        return status switch
        {
            PaymentStatus.Confirmed => builder.AsConfirmed().Build(),
            PaymentStatus.Cancelled => builder.AsCancelled().Build(),
            _ => builder.Build()
        };
    }
}