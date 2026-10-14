using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class CustomerInfoTests
{
    [Fact]
    public void GivenFormattedPhoneAndPaddedName_WhenCreatingCustomer_ThenNormalizesBoth()
    {
        // Arrange
        const string name = "  João Silva  ";
        const string phone = "(11) 98765-4321";

        // Act
        var customer = CustomerInfo.Create(name, phone);

        // Assert
        customer.Name.ShouldBe("João Silva");
        customer.Phone.ShouldNotBeNull().Number.ShouldBe("11987654321");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenBlankPhone_WhenCreatingCustomer_ThenPhoneIsNull(string? phone)
    {
        // Arrange
        const string name = "João Silva";

        // Act
        var customer = CustomerInfo.Create(name, phone);

        // Assert
        customer.Phone.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Jo")]
    public void GivenInvalidName_WhenCreatingCustomer_ThenThrowsDomainException(string name)
    {
        // Arrange
        const string phone = "11987654321";

        // Act
        var exception = Record.Exception(() => CustomerInfo.Create(name, phone));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("119876543210")]
    [InlineData("abc")]
    public void GivenPhoneWithoutTenOrElevenDigits_WhenCreatingCustomer_ThenThrowsDomainException(string phone)
    {
        // Arrange
        const string name = "João Silva";

        // Act
        var exception = Record.Exception(() => CustomerInfo.Create(name, phone));

        // Assert
        exception.ShouldBeOfType<DomainException>();
    }
}