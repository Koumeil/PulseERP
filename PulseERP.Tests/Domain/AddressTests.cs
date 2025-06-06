namespace PulseERP.Tests.Domain;

using System;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;
using Xunit;

public class AddressTests
{
    [Fact]
    public void Create_ValidAddress_ShouldSucceed()
    {
        var address = new Address("123 Main St", "Brussels", "1000", "BE");

        Assert.Equal("123 Main St", address.Street);
        Assert.Equal("Brussels", address.City);
        Assert.Equal("1000", address.PostalCode);
        Assert.Equal("BE", address.Country);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_InvalidStreet_ShouldThrow(string? invalidStreet)
    {
        Assert.Throws<DomainValidationException>(() =>
            new Address(invalidStreet!, "City", "1000", "BE")
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_InvalidCity_ShouldThrow(string? invalidCity)
    {
        Assert.Throws<DomainValidationException>(() =>
            new Address("123 Main St", invalidCity!, "1000", "BE")
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_InvalidPostalCode_ShouldThrow(string? invalidPostal)
    {
        Assert.Throws<DomainValidationException>(() =>
            new Address("123 Main St", "City", invalidPostal!, "BE")
        );
    }

    [Fact]
    public void Create_TooLongPostalCode_ShouldThrow()
    {
        var longPostal = new string('1', 21); // 21 chars
        Assert.Throws<DomainValidationException>(() =>
            new Address("123 Main St", "City", longPostal, "BE")
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_InvalidCountry_ShouldThrow(string? invalidCountry)
    {
        // Arrange & Act
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Address("123 Main St", "City", "1000", invalidCountry!)
        );

        // Assert
        Assert.Equal("Country cannot be null or whitespace.", exception.Message);
    }

    [Fact]
    public void Addresses_WithSameValues_ShouldBeEqual()
    {
        var a1 = new Address("123 Main St", "City", "1000", "BE");
        var a2 = new Address("123 Main St", "City", "1000", "BE");

        Assert.Equal(a1, a2);
        Assert.True(a1 == a2);
        Assert.False(a1 != a2);
        Assert.Equal(a1.GetHashCode(), a2.GetHashCode());
    }

    [Fact]
    public void Addresses_WithDifferentValues_ShouldNotBeEqual()
    {
        var a1 = new Address("123 Main St", "City", "1000", "BE");
        var a2 = new Address("124 Main St", "City", "1000", "BE");

        Assert.NotEqual(a1, a2);
        Assert.True(a1 != a2);
        Assert.False(a1 == a2);
    }

    [Fact]
    public void ToString_ShouldReturnMultilineFormattedString()
    {
        var address = new Address("123 Main St", "Brussels", "1000", "Belgium");

        var expected = $"123 Main St{Environment.NewLine}1000 Brussels{Environment.NewLine}Belgium";
        Assert.Equal(expected, address.ToString());
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var a = new Address("1", "C", "1000", "BE");
        Assert.False(a.Equals(null));
    }

    [Fact]
    public void Equals_IsSymmetric()
    {
        var a1 = new Address("1", "C", "1000", "BE");
        var a2 = new Address("1", "C", "1000", "BE");

        Assert.True(a1.Equals(a2));
        Assert.True(a2.Equals(a1));
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var address = new Address("1", "C", "1000", "BE");
        var notAddress = "not an address";

        Assert.False(address.Equals(notAddress));
    }
}
