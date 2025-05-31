using FluentAssertions;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class AddressTests
{
    [Fact]
    public void Create_Should_Parse_Raw() =>
        Address
            .Create("12 rue ABC, Paris, 75001, France")
            .ToString()
            .Should()
            .Be("12 rue ABC, Paris, 75001, France");

    [Fact]
    public void Invalid_Format_Should_Throw() =>
        FluentActions
            .Invoking(() => Address.Create("Rue seule"))
            .Should()
            .Throw<ArgumentException>();

    [Fact]
    public void Update_Should_Be_Idempotent()
    {
        var addr = Address.Create("A, B, C, D");

        // Act
        var updated = addr.Update();

        // Assert : égalité de valeur, pas forcément de référence
        updated.Should().Be(addr); // mêmes champs
        updated.Equals(addr).Should().BeTrue();
    }
}
