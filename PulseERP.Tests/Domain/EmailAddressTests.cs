using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("user+alias@sub.domain.org")]
    public void Constructor_ValidEmails_ShouldCreateLowercased(string input)
    {
        // Act
        var email = new EmailAddress(input);

        // Assert
        Assert.Equal(input.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_NullOrWhitespace_ShouldThrow(string? input)
    {
        Assert.Throws<DomainValidationException>(() => new EmailAddress(input!));
    }

    [Theory]
    [InlineData("no-at-symbol")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user@@domain.com")]
    [InlineData("user@domain..com")]
    public void Constructor_InvalidFormat_ShouldThrow(string invalidEmail)
    {
        var ex = Assert.Throws<DomainValidationException>(() => new EmailAddress(invalidEmail));
        Assert.Contains("Invalid email format", ex.Message);
    }

    [Fact]
    public void Equals_SameValue_ShouldReturnTrue()
    {
        var email1 = new EmailAddress("same@domain.com");
        var email2 = new EmailAddress("same@domain.com");

        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equals_DifferentCase_ShouldReturnTrue()
    {
        var email1 = new EmailAddress("User@domain.com");
        var email2 = new EmailAddress("user@DOMAIN.com");

        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse()
    {
        var email1 = new EmailAddress("user1@domain.com");
        var email2 = new EmailAddress("user2@domain.com");

        Assert.NotEqual(email1, email2);
        Assert.True(email1 != email2);
        Assert.False(email1 == email2);
    }

    [Fact]
    public void ToString_ShouldReturnLowerCasedValue()
    {
        var email = new EmailAddress("Upper@Email.COM");
        Assert.Equal("upper@email.com", email.ToString());
    }

    [Fact]
    public void GetHashCode_ShouldBeEqualForSameValues()
    {
        var email1 = new EmailAddress("hash@test.com");
        var email2 = new EmailAddress("HASH@test.com");

        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }
}
