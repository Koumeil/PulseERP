using FluentAssertions;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class RoleTests
{
    [Fact]
    public void Same_Name_Ignoring_Case_Should_Be_Equal()
    {
        var r1 = Role.Create("Admin");
        var r2 = Role.Create("admin");
        r1.Should().Be(r2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrNullInput_ShouldReturn_DefaultUser(string? raw)
    {
        // Act
        Role result = Role.Create(raw);

        // Assert
        result.Should().Be(Role.User);
    }

    [Fact]
    public void Default_Create_Should_Return_User()
    {
        var role = Role.Create(); // aucun paramètre
        role.Should().Be(Role.User);
    }

    [Fact]
    public void Explicit_Null_Or_Empty_Should_Return_User()
    {
        Role.Create(null!).Should().Be(Role.User);
        Role.Create("").Should().Be(Role.User);
    }

    [Fact]
    public void Case_Insensitive_Equality()
    {
        var r1 = Role.Create("Admin");
        var r2 = Role.Create("admin");
        r1.Should().Be(r2);
    }
}
