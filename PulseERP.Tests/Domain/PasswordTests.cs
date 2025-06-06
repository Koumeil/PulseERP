using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class PasswordTests
{
    [Fact]
    public void Constructor_ValidPassword_ShouldSucceed()
    {
        var password = new Password("P@ssw0rd10!");
        Assert.False(string.IsNullOrWhiteSpace(password.HashedValue));
        Assert.Contains(":", password.HashedValue); // Format salt:hash
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("      ")]
    public void Constructor_NullOrWhitespace_ShouldThrow(string? input)
    {
        Assert.Throws<DomainValidationException>(() => new Password(input!));
    }

    [Theory]
    [InlineData("short1!A")] // < 10 chars
    [InlineData("nouppercase1!")] // no uppercase
    [InlineData("NOLOWERCASE1!")] // no lowercase
    [InlineData("NoNumber!@")] // no number
    [InlineData("NoSpecial123")] // no special char
    public void Constructor_InvalidComplexity_ShouldThrow(string raw)
    {
        Assert.Throws<DomainValidationException>(() => new Password(raw));
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var raw = "Str0ngP@ss!";
        var password = new Password(raw);
        var result = password.Verify(raw);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var password = new Password("Secur3P@ss!");
        Assert.False(password.Verify("WrongPass123!"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_NullOrWhitespaceInput_ShouldReturnFalse(string? input)
    {
        var password = new Password("V@lid12345!");
        Assert.False(password.Verify(input!));
    }

    [Fact]
    public void Password_Equality_ShouldMatchByHash()
    {
        var raw = "Uniqu3P@ss!";
        var p1 = new Password(raw);
        var p2 = new Password(raw); // Salted => hash should differ

        // Different hashes due to salt
        Assert.NotEqual(p1.HashedValue, p2.HashedValue);
        Assert.False(p1 == p2);
    }

    [Fact]
    public void Equality_Operators_ShouldWork()
    {
        var p1 = new Password("Test1@Pass!");
        var p2 = p1;
        var p3 = new Password("Another1@!");

        Assert.True(p1 == p2);
        Assert.True(p1.Equals(p2));
        Assert.False(p1 == p3);
    }

    [Fact]
    public void ToString_ShouldReturnHashedValue()
    {
        var password = new Password("MyP@ssw0rd10!");
        var str = password.ToString();

        Assert.Equal(password.HashedValue, str);
    }
}
