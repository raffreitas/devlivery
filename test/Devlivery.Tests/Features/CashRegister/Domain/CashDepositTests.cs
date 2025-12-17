using Devlivery.Features.CashRegister.Domain;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Domain;

[Collection("CashRegister Unit Tests")]
[Trait("Category", "Unit Tests")]
public sealed class CashDepositTests(CashRegisterUnitTestFixture fixture)
{
    [Fact]
    public void Constructor_Should_Create_CashDeposit_With_Correct_Properties()
    {
        // Arrange
        var cashSessionId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var attendantId = Guid.NewGuid();
        const string attendantName = "Maria Santos";
        const decimal amount = 100.00m;
        const string notes = "Depósito de sangria";

        // Act
        var deposit = new CashDeposit(
            cashSessionId,
            establishmentId,
            attendantId,
            attendantName,
            amount,
            notes);

        // Assert
        deposit.CashSessionId.ShouldBe(cashSessionId);
        deposit.EstablishmentId.ShouldBe(establishmentId);
        deposit.AttendantId.ShouldBe(attendantId);
        deposit.AttendantName.ShouldBe(attendantName);
        deposit.Amount.ShouldBe(amount);
        deposit.Notes.ShouldBe(notes);
        deposit.DepositedAt.ShouldNotBe(default);
        deposit.CreatedAt.ShouldNotBe(default);
        deposit.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Notes()
    {
        // Arrange & Act
        var deposit = fixture.CreateCashDeposit(notes: null);

        // Assert
        deposit.Notes.ShouldBeNull();
    }

    [Theory]
    [InlineData(10.00)]
    [InlineData(50.50)]
    [InlineData(100.00)]
    [InlineData(500.99)]
    public void Constructor_Should_Accept_Various_Amounts(decimal amount)
    {
        // Arrange & Act
        var deposit = fixture.CreateCashDeposit(amount: amount);

        // Assert
        deposit.Amount.ShouldBe(amount);
    }
}