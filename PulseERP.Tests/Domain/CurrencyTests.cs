using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class CurrencyTests
{
    [Fact]
    public void Equality_SameCodeDifferentCase_ShouldBeEqual()
    {
        var c1 = new Currency("usd");
        var c2 = new Currency("USD");

        Assert.Equal(c1, c2);
        Assert.True(c1.Equals(c2));
        Assert.Equal(c1.GetHashCode(), c2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentCode_ShouldNotBeEqual()
    {
        var c1 = new Currency("USD");
        var c2 = new Currency("EUR");

        Assert.NotEqual(c1, c2);
        Assert.False(c1.Equals(c2));
    }

    [Fact]
    public void ToString_ShouldReturnCode()
    {
        var c = new Currency("gbp");
        Assert.Equal("GBP", c.ToString());
    }

    [Theory]
    [InlineData("usd")]
    [InlineData(" EUR ")]
    [InlineData("jpy")]
    public void Constructor_ValidCurrencyCode_ShouldNormalizeToUpper(string input)
    {
        var currency = new Currency(input);
        Assert.Equal(input.Trim().ToUpperInvariant(), currency.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("EU")] // trop court
    [InlineData("EURO")] // trop long
    public void Constructor_InvalidCode_ShouldThrow(string? input)
    {
        Assert.Throws<DomainValidationException>(() => new Currency(input!));
    }
}
