using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class RoleTests
{
    [Theory]
    [InlineData("Admin", "Admin")]
    [InlineData("  manager  ", "Manager")]
    [InlineData("SYSTEM ADMIN", "System Admin")]
    [InlineData("custoMer support", "Customer Support")]
    public void Constructor_ValidInput_ShouldNormalizeAndStoreTitleCase(
        string input,
        string expected
    )
    {
        var role = new Role(input);

        Assert.Equal(expected, role.Value);
        Assert.Equal(expected, role.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidNullOrWhitespace_ShouldThrow(string? input)
    {
        var ex = Assert.Throws<DomainValidationException>(() => new Role(input!));
        Assert.Contains("null or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_TooLong_ShouldThrow()
    {
        var input = new string('x', 51);
        var ex = Assert.Throws<DomainValidationException>(() => new Role(input));
        Assert.Contains("exceed 50 characters", ex.Message);
    }

    [Fact]
    public void Equality_SameNormalizedValue_DifferentCases_ShouldBeEqual()
    {
        var role1 = new Role("admin");
        var role2 = new Role("ADMIN");

        Assert.Equal(role1, role2);
        Assert.True(role1 == role2);
        Assert.False(role1 != role2);
        Assert.Equal(role1.GetHashCode(), role2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var role1 = new Role("Manager");
        var role2 = new Role("Developer");

        Assert.NotEqual(role1, role2);
        Assert.False(role1 == role2);
        Assert.True(role1 != role2);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var role = new Role(" support agent ");
        Assert.Equal("Support Agent", role.ToString());
    }

    [Fact]
    public void Equals_WithObject_ShouldReturnTrueForSameValue()
    {
        Role role1 = new("admin");
        object role2 = new Role("ADMIN");

        Assert.True(role1.Equals(role2));
    }

    [Fact]
    public void GetHashCode_ShouldBeCaseInsensitive()
    {
        Role role1 = new("adMin");
        Role role2 = new("ADMIN");

        Assert.Equal(role1.GetHashCode(), role2.GetHashCode());
    }

    [Fact]
    public void Struct_Default_ShouldHaveEmptyValue()
    {
        Role role = default;
        Assert.Null(role.Value); // car struct et non class → pas de valeur assignée par défaut
    }
}
