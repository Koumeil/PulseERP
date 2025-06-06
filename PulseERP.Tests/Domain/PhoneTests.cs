using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain;

public class PhoneTests
{
    [Theory]
    [InlineData("+32155552671")]
    [InlineData("+33123456789")]
    [InlineData("+447911123456")]
    public void Constructor_ValidPhone_ShouldSucceed(string validPhone)
    {
        var phone = new Phone(validPhone);

        Assert.Equal(validPhone, phone.Value);
        Assert.Equal(validPhone, phone.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Constructor_NullOrWhitespace_ShouldThrow(string? input)
    {
        Assert.Throws<DomainValidationException>(() => new Phone(input!));
    }

    [Theory]
    [InlineData("0612345678")] // French local format
    [InlineData("1234567890")] // Missing '+'
    [InlineData("+")] // Too short
    [InlineData("+1")] // Too short
    [InlineData("+12abc567890")] // Contains letters
    [InlineData("+1234567890123456")] // Too long (16 digits)
    public void Constructor_InvalidFormat_ShouldThrow(string input)
    {
        Assert.Throws<DomainValidationException>(() => new Phone(input));
    }

    [Fact]
    public void Equals_TwoSamePhones_ShouldBeEqual()
    {
        var phone1 = new Phone("+1234567890");
        var phone2 = new Phone("+1234567890");

        Assert.True(phone1 == phone2);
        Assert.False(phone1 != phone2);
        Assert.True(phone1.Equals(phone2));
        Assert.Equal(phone1.GetHashCode(), phone2.GetHashCode());
    }

    [Fact]
    public void Equals_TwoDifferentPhones_ShouldNotBeEqual()
    {
        var phone1 = new Phone("+3234567890");
        var phone2 = new Phone("+4487654321");

        Assert.False(phone1 == phone2);
        Assert.True(phone1 != phone2);
        Assert.False(phone1.Equals(phone2));
    }

    [Fact]
    public void ToString_ShouldReturnOriginalValue()
    {
        var phone = new Phone("+14150000000");
        Assert.Equal("+14150000000", phone.ToString());
    }
}
