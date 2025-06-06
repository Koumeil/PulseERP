using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class ProductDescriptionTests
{
    public static IEnumerable<object[]> ValidDescriptions =>
        new List<object[]>
        {
            new object[] { "" },
            new object[] { "Short description." },
            new object[] { "  Trimmed input should work fine.  " },
            new object[] { "D" + new string('x', 999) }, // dynamique
        };

    [Theory]
    [MemberData(nameof(ValidDescriptions))]
    public void Constructor_ValidDescriptions_ShouldSucceed(string input)
    {
        var desc = new ProductDescription(input);

        Assert.NotNull(desc);
        Assert.True(desc.Value.Length <= 1000);
        Assert.Equal(input.Trim(), desc.Value);
    }

    [Fact]
    public void Constructor_NullInput_ShouldThrow()
    {
        Assert.Throws<DomainValidationException>(() => new ProductDescription(null!));
    }

    [Fact]
    public void Constructor_TooLongInput_ShouldThrow()
    {
        var longInput = new string('x', 1001); // > 1000 chars
        Assert.Throws<DomainValidationException>(() => new ProductDescription(longInput));
    }

    [Fact]
    public void Equals_SameValue_ShouldBeEqual()
    {
        var a = new ProductDescription("Identical value");
        var b = new ProductDescription("Identical value");

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_ShouldNotBeEqual()
    {
        var a = new ProductDescription("One");
        var b = new ProductDescription("Two");

        Assert.False(a == b);
        Assert.True(a != b);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ShouldReturnDescriptionValue()
    {
        var text = "Product desc";
        var desc = new ProductDescription(text);

        Assert.Equal(text, desc.ToString());
    }
}
