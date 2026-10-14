using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Domain.SeedWork;
using Devlivery.Tests.Common.Builders;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Domain;

[Trait("Category", "Unit Tests")]
public sealed class CashSessionTests
{
    private static readonly Guid Operator = Guid.NewGuid();

    [Fact]
    public void GivenOpeningData_WhenOpeningSession_ThenStartsOpenWithOpeningAmountAsExpectedCash()
    {
        // Arrange
        var establishmentId = Guid.NewGuid();
        var attendantId = Guid.NewGuid();

        // Act
        var session = new CashSession(establishmentId, attendantId, "João Silva", 100m, "Turno da noite");

        // Assert
        session.Status.ShouldBe(CashSessionStatus.Open);
        session.EstablishmentId.ShouldBe(establishmentId);
        session.AttendantId.ShouldBe(attendantId);
        session.AttendantName.ShouldBe("João Silva");
        session.OpeningAmount.ShouldBe(100m);
        session.Notes.ShouldBe("Turno da noite");
        session.ExpectedCashAmount.ShouldBe(100m);
        session.TotalRevenue.ShouldBe(0m);
        session.Movements.ShouldBeEmpty();
        session.EndAt.ShouldBeNull();
    }

    [Fact]
    public void GivenOpenSession_WhenAddingPayment_ThenRecordsMovementAttributedToAuthor()
    {
        // Arrange
        var session = new CashSessionBuilder().Build();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act
        session.AddPayment(paymentId, 79m, PaymentMethod.Pix, orderId, Operator);

        // Assert
        var movement = session.Movements.ShouldHaveSingleItem();
        movement.EntryType.ShouldBe(CashSessionEntryType.Payment);
        movement.Amount.ShouldBe(79m);
        movement.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        movement.OrderPaymentId.ShouldBe(paymentId);
        movement.RelatedOrderId.ShouldBe(orderId);
        movement.CreatedBy.ShouldBe(Operator);
        movement.EstablishmentId.ShouldBe(session.EstablishmentId);
        session.TotalRevenue.ShouldBe(79m);
        session.TotalOrders.ShouldBe(1);
    }

    [Fact]
    public void GivenAlreadyRecordedPayment_WhenAddingSamePaymentAgain_ThenRecordsItOnlyOnce()
    {
        // Arrange
        var session = new CashSessionBuilder().Build();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        session.AddPayment(paymentId, 79m, PaymentMethod.Pix, orderId, Operator);

        // Act
        session.AddPayment(paymentId, 79m, PaymentMethod.Pix, orderId, Guid.NewGuid());

        // Assert
        session.Movements.ShouldHaveSingleItem();
        session.TotalRevenue.ShouldBe(79m);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash, 150)]
    [InlineData(PaymentMethod.Pix, 100)]
    [InlineData(PaymentMethod.CreditCard, 100)]
    [InlineData(PaymentMethod.DebitCard, 100)]
    public void GivenPaymentMethod_WhenAddingPayment_ThenOnlyCashChangesExpectedCash(
        PaymentMethod method, decimal expectedCash)
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();

        // Act
        session.AddPayment(Guid.NewGuid(), 50m, method, Guid.NewGuid(), Operator);

        // Assert
        session.ExpectedCashAmount.ShouldBe(expectedCash);
        session.TotalRevenue.ShouldBe(50m);
    }

    [Fact]
    public void GivenRecordedPayment_WhenReversingItTwice_ThenRecordsSingleRefundAndNetsRevenue()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        session.AddPayment(paymentId, 50m, PaymentMethod.Cash, orderId, Operator);

        // Act
        session.AddReversal(paymentId, 50m, PaymentMethod.Cash, "Pedido Cancelado", orderId, Operator);
        session.AddReversal(paymentId, 50m, PaymentMethod.Cash, "Pedido Cancelado", orderId, Operator);

        // Assert
        var refund = session.Movements.Where(m => m.EntryType == CashSessionEntryType.Refund).ShouldHaveSingleItem();
        refund.Reason.ShouldBe("Pedido Cancelado");
        refund.OrderPaymentId.ShouldBe(paymentId);
        session.HasReversalFor(paymentId).ShouldBeTrue();
        session.TotalRevenue.ShouldBe(0m);
        session.ExpectedCashAmount.ShouldBe(100m);
    }

    [Fact]
    public void GivenChangeForOrder_WhenAddingItTwice_ThenRecordsSingleChangeAndReducesExpectedCash()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();
        var orderId = Guid.NewGuid();

        // Act
        session.AddChange(orderId, 30m, Operator);
        session.AddChange(orderId, 30m, Operator);

        // Assert
        var change = session.Movements.ShouldHaveSingleItem();
        change.EntryType.ShouldBe(CashSessionEntryType.Change);
        change.RelatedOrderId.ShouldBe(orderId);
        session.HasChangeFor(orderId).ShouldBeTrue();
        session.ExpectedCashAmount.ShouldBe(70m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void GivenNonPositiveChange_WhenAddingChange_ThenIgnoresIt(decimal amount)
    {
        // Arrange
        var session = new CashSessionBuilder().Build();

        // Act
        session.AddChange(Guid.NewGuid(), amount, Operator);

        // Assert
        session.Movements.ShouldBeEmpty();
    }

    [Fact]
    public void GivenOpenSession_WhenAddingDeposit_ThenIncreasesExpectedCash()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();

        // Act
        var deposit = session.AddDeposit(50m, Operator, "Troco inicial");

        // Assert
        deposit.EntryType.ShouldBe(CashSessionEntryType.Deposit);
        deposit.CreatedBy.ShouldBe(Operator);
        deposit.Reason.ShouldBe("Troco inicial");
        session.Movements.ShouldContain(deposit);
        session.TotalDeposits().ShouldBe(50m);
        session.ExpectedCashAmount.ShouldBe(150m);
    }

    [Fact]
    public void GivenMixedMovements_WhenComputingTotals_ThenNetsPaymentsRefundsChangeAndDeposits()
    {
        // Arrange
        var session = new CashSessionBuilder().WithOpeningAmount(100m).Build();
        var cashOrder = Guid.NewGuid();
        var refundedPayment = Guid.NewGuid();
        session.AddPayment(Guid.NewGuid(), 80m, PaymentMethod.Cash, cashOrder, Operator);
        session.AddPayment(Guid.NewGuid(), 30m, PaymentMethod.Pix, Guid.NewGuid(), Operator);
        session.AddPayment(refundedPayment, 10m, PaymentMethod.Cash, Guid.NewGuid(), Operator);
        session.AddReversal(refundedPayment, 10m, PaymentMethod.Cash, "Pedido Cancelado", Guid.NewGuid(), Operator);
        session.AddChange(cashOrder, 20m, Operator);
        session.AddDeposit(25m, Operator, null);

        // Act
        var totals = new
        {
            session.TotalRevenue,
            CashPayments = session.TotalCashPayments(),
            Deposits = session.TotalDeposits(),
            session.ExpectedCashAmount,
            session.TotalOrders
        };

        // Assert
        totals.TotalRevenue.ShouldBe(90m); // 120 paid - 10 refunded - 20 change
        totals.CashPayments.ShouldBe(60m); // 90 cash - 10 refunded - 20 change
        totals.Deposits.ShouldBe(25m);
        totals.ExpectedCashAmount.ShouldBe(185m); // 100 opening + 25 deposit + 60 cash
        totals.TotalOrders.ShouldBe(3);
    }

    [Theory]
    [InlineData("payment")]
    [InlineData("reversal")]
    [InlineData("change")]
    public void GivenClosedSession_WhenAddingOrderMovement_ThenThrowsDomainException(string movement)
    {
        // Arrange
        var session = new CashSessionBuilder().AsClosed().Build();

        // Act
        var exception = Record.Exception(() => AddMovement(session, movement, 10m, Operator));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldContain("caixa fechado");
        session.Movements.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("payment")]
    [InlineData("reversal")]
    [InlineData("deposit")]
    public void GivenNegativeAmount_WhenAddingMovement_ThenThrowsDomainException(string movement)
    {
        // Arrange
        var session = new CashSessionBuilder().Build();

        // Act
        var exception = Record.Exception(() => AddMovement(session, movement, -1m, Operator));

        // Assert
        exception.ShouldBeOfType<DomainException>();
        session.Movements.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("payment")]
    [InlineData("reversal")]
    [InlineData("change")]
    [InlineData("deposit")]
    public void GivenMissingAuthor_WhenAddingMovement_ThenThrowsDomainException(string movement)
    {
        // Arrange
        var session = new CashSessionBuilder().Build();

        // Act
        var exception = Record.Exception(() => AddMovement(session, movement, 10m, Guid.Empty));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("O autor é obrigatório.");
    }

    [Fact]
    public void GivenOpenSession_WhenClosing_ThenRecordsClosingAmountNotesAndEndTime()
    {
        // Arrange
        var session = new CashSessionBuilder().WithNotes("Abertura").Build();

        // Act
        session.Close(150m, "Sem divergências");

        // Assert
        session.Status.ShouldBe(CashSessionStatus.Closed);
        session.ClosingAmount.ShouldBe(150m);
        session.Notes.ShouldBe("Sem divergências");
        session.EndAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenBlankNotes_WhenClosing_ThenKeepsOpeningNotes(string? notes)
    {
        // Arrange
        var session = new CashSessionBuilder().WithNotes("Abertura").Build();

        // Act
        session.Close(150m, notes);

        // Assert
        session.Notes.ShouldBe("Abertura");
    }

    [Fact]
    public void GivenClosedSession_WhenClosingAgain_ThenThrowsDomainException()
    {
        // Arrange
        var session = new CashSessionBuilder().AsClosed(100m).Build();

        // Act
        var exception = Record.Exception(() => session.Close(200m, null));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Este caixa já está fechado.");
        session.ClosingAmount.ShouldBe(100m);
    }

    private static void AddMovement(CashSession session, string movement, decimal amount, Guid createdBy)
    {
        switch (movement)
        {
            case "payment":
                session.AddPayment(Guid.NewGuid(), amount, PaymentMethod.Cash, Guid.NewGuid(), createdBy);
                break;
            case "reversal":
                session.AddReversal(Guid.NewGuid(), amount, PaymentMethod.Cash, "Pedido Cancelado", Guid.NewGuid(),
                    createdBy);
                break;
            case "change":
                session.AddChange(Guid.NewGuid(), amount, createdBy);
                break;
            case "deposit":
                session.AddDeposit(amount, createdBy, null);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(movement), movement, null);
        }
    }
}