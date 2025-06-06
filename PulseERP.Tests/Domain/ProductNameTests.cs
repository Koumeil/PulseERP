using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class ProductNameTests
{
    [Theory]
    [InlineData("Product A")]
    [InlineData("   Trimmed Name   ")]
    [InlineData("A")] // min length
    [InlineData(
        "Xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    )] // 200 chars
    public void Constructor_ValidInput_ShouldSucceed(string input)
    {
        var name = new ProductName(input);

        Assert.NotNull(name);
        Assert.True(name.Value.Length <= 200);
        Assert.Equal(input.Trim(), name.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void Constructor_Invalid_NullEmptyOrWhitespace_ShouldThrow(string? input)
    {
        Assert.Throws<DomainValidationException>(() => new ProductName(input!));
    }

    [Fact]
    public void Constructor_TooLong_ShouldThrow()
    {
        var tooLong = new string('X', 201);
        var ex = Assert.Throws<DomainValidationException>(() => new ProductName(tooLong));
        Assert.Contains("200", ex.Message);
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var name1 = new ProductName("Test Product");
        var name2 = new ProductName("Test Product");

        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
        Assert.False(name1 != name2);
        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        var name1 = new ProductName("Product A");
        var name2 = new ProductName("Product B");

        Assert.NotEqual(name1, name2);
        Assert.False(name1 == name2);
        Assert.True(name1 != name2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var name = new ProductName("Some Product");
        Assert.Equal("Some Product", name.ToString());
    }
}
