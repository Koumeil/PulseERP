using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class EmailAddressTests
{
    [Fact]
    public void Valid_Email_Should_Create() =>
        EmailAddress.Create("TEST@Example.com").Value.Should().Be("test@example.com");

    [Fact]
    public void Invalid_Email_Should_Throw() =>
        FluentActions.Invoking(() => EmailAddress.Create("bad")).Should().Throw<DomainException>();

    [Fact]
    public void Update_Same_Value_Should_Return_Same_Instance()
    {
        var e = EmailAddress.Create("a@b.com");
        e.Update("A@B.com").Should().BeSameAs(e);
    }
}
