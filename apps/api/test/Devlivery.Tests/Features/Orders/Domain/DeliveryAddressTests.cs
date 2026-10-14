using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.SeedWork;

using Shouldly;

namespace Devlivery.Tests.Features.Orders.Domain;

[Trait("Category", "Unit Tests")]
public sealed class DeliveryAddressTests
{
    [Fact]
    public void GivenPaddedAddressAndReference_WhenCreatingAddress_ThenTrimsBoth()
    {
        // Arrange
        const string fullAddress = "  Rua das Flores, 123  ";
        const string reference = "  Portão azul ";

        // Act
        var address = new DeliveryAddress(fullAddress, reference);

        // Assert
        address.FullAddress.ShouldBe("Rua das Flores, 123");
        address.Reference.ShouldBe("Portão azul");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenBlankAddress_WhenCreatingAddress_ThenThrowsDomainException(string fullAddress)
    {
        // Arrange
        const string reference = "Portão azul";

        // Act
        var exception = Record.Exception(() => new DeliveryAddress(fullAddress, reference));

        // Assert
        exception.ShouldBeOfType<DomainException>().Message.ShouldBe("Endereço de entrega é obrigatório");
    }
}