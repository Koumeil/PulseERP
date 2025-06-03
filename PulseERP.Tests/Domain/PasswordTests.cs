using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Tests.Domain;

public class PasswordTests
{
    [Fact]
    public void Strong_Password_Should_Create() => Password.Create("Abcdef1!").Should().NotBeNull();

    [Theory]
    [InlineData("short1!")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWER1!")]
    [InlineData("NoDigit!!")]
    public void Weak_Password_Should_Throw(string pwd) =>
        FluentActions.Invoking(() => Password.Create(pwd)).Should().Throw<DomainException>();
}
