using Devlivery.Common.Domain.Enums;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Enums;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class OrderPaymentTests
{
    [Fact]
    public void Confirm_Should_Set_Status_And_Timestamps()
    {
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 10m);

        payment.Confirm();

        payment.PaymentStatus.ShouldBe(PaymentStatus.Confirmed);
        payment.ConfirmedAt.ShouldNotBeNull();
        payment.UpdatedAt.ShouldBeGreaterThan(payment.CreatedAt);
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 10m);

        payment.Cancel();

        payment.PaymentStatus.ShouldBe(PaymentStatus.Cancelled);
        payment.UpdatedAt.ShouldBeGreaterThan(payment.CreatedAt);
    }

    [Fact]
    public void Update_Should_Change_Method_And_Amount_When_Pending()
    {
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 10m);

        payment.Update(PaymentMethod.Pix, 15m);

        payment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        payment.Amount.ShouldBe(15m);
    }

    [Fact]
    public void Update_Should_Throw_When_Confirmed()
    {
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 10m);
        payment.Confirm();

        Should.Throw<InvalidOperationException>(() => payment.Update(PaymentMethod.Pix, 5m));
    }

    [Fact]
    public void Confirm_Should_Throw_When_Cancelled()
    {
        var payment = new OrderPayment(Guid.NewGuid(), PaymentMethod.Cash, 10m);
        payment.Cancel();

        Should.Throw<InvalidOperationException>(() => payment.Confirm());
    }
}
