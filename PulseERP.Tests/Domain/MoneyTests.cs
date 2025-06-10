using System.Globalization;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class MoneyTests
{
    private readonly Currency _eur = new("EUR");
    private readonly Currency _usd = new("USD");

    [Fact]
    public void Constructor_Valid_ShouldCreate()
    {
        var money = new Money(100.50m, _eur);
        Assert.Equal(100.50m, money.Amount);
        Assert.Equal(_eur, money.Currency);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_NegativeAmount_ShouldThrow(decimal amount)
    {
        Assert.Throws<DomainValidationException>(() => new Money(amount, _eur));
    }

    [Fact]
    public void Constructor_NullCurrency_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new Money(10m, null!));
    }

    [Fact]
    public void Add_SameCurrency_ShouldAddAmounts()
    {
        var m1 = new Money(20m, _eur);
        var m2 = new Money(30m, _eur);
        var result = m1.Add(m2);

        Assert.Equal(50m, result.Amount);
        Assert.Equal(_eur, result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var m1 = new Money(10m, _eur);
        var m2 = new Money(10m, _usd);

        Assert.Throws<DomainValidationException>(() => m1.Add(m2));
    }

    [Fact]
    public void Subtract_SameCurrency_ValidResult()
    {
        var m1 = new Money(50m, _eur);
        var m2 = new Money(20m, _eur);
        var result = m1.Subtract(m2);

        Assert.Equal(30m, result.Amount);
    }

    [Fact]
    public void Subtract_NegativeResult_ShouldThrow()
    {
        var m1 = new Money(10m, _eur);
        var m2 = new Money(20m, _eur);

        Assert.Throws<DomainValidationException>(() => m1.Subtract(m2));
    }

    [Theory]
    [InlineData(2, 20)]
    [InlineData(0.5, 5)]
    public void Multiply_ValidFactor_ShouldMultiply(decimal factor, decimal expected)
    {
        var m = new Money(10m, _eur);
        var result = m.Multiply(factor);

        Assert.Equal(expected, result.Amount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Multiply_NegativeFactor_ShouldThrow(decimal factor)
    {
        var m = new Money(10m, _eur);
        Assert.Throws<DomainValidationException>(() => m.Multiply(factor));
    }

    [Theory]
    [InlineData(0.5, 20)] // 10 / 0.5 = 20
    [InlineData(2, 5)]     // 10 / 2 = 5
    public void Divide_ValidDivisor_ShouldDivide(decimal divisor, decimal expected)
    {
        var m = new Money(10m, _eur);
        var result = m.Divide(divisor);

        Assert.Equal(expected, result.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Divide_ByZeroOrNegative_ShouldThrow(decimal divisor)
    {
        var m = new Money(10m, _eur);
        Assert.Throws<DomainValidationException>(() => m.Divide(divisor));
    }

    [Fact]
    public void IsZero_ZeroAmount_ShouldReturnTrue()
    {
        var m = new Money(0m, _eur);
        Assert.True(m.IsZero());
    }

    [Fact]
    public void IsGreaterThan_ShouldReturnExpected()
    {
        var m1 = new Money(20m, _eur);
        var m2 = new Money(10m, _eur);

        Assert.True(m1.IsGreaterThan(m2));
        Assert.False(m2.IsGreaterThan(m1));
    }

    [Fact]
    public void IsLessThan_ShouldReturnExpected()
    {
        var m1 = new Money(5m, _eur);
        var m2 = new Money(10m, _eur);

        Assert.True(m1.IsLessThan(m2));
    }

    [Fact]
    public void ToString_ShouldRespectCulture()
    {
        var m = new Money(1234.56m, _eur);
        var formatted = m.ToString(new CultureInfo("fr-FR"));

        // Remplace l’espace insécable (U+202F) par un espace normal (U+0020)
        var normalized = formatted.Replace('\u202F', ' ');

        Assert.Equal("1 234,56 EUR", normalized);
    }

    [Fact]
    public void CompareTo_ShouldReturnCorrectOrder()
    {
        var m1 = new Money(5m, _eur);
        var m2 = new Money(10m, _eur);

        Assert.True(m1.CompareTo(m2) < 0);
        Assert.True(m2.CompareTo(m1) > 0);
        Assert.Equal(0, m1.CompareTo(new Money(5m, _eur)));
    }

    [Fact]
    public void Equality_Operators_ShouldWork()
    {
        var m1 = new Money(5m, _eur);
        var m2 = new Money(5m, _eur);
        var m3 = new Money(10m, _eur);

        Assert.True(m1 == m2);
        Assert.False(m1 == m3);
        Assert.True(m1 != m3);
    }

    [Fact]
    public void DifferentCurrency_Comparison_ShouldThrow()
    {
        var m1 = new Money(5m, _eur);
        var m2 = new Money(5m, _usd);

        Assert.Throws<DomainValidationException>(() => m1.CompareTo(m2));
        Assert.Throws<DomainValidationException>(() => m1.IsGreaterThan(m2));
    }
}
