using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class PhoneTests
{
    [Theory]
    [InlineData("+32485123456")]
    [InlineData("0612345678")]
    public void Valid_Phone_Should_Create(string num) =>
        Phone.Create(num).Value.Should().Be(num.Trim());

    [Fact]
    public void Invalid_Phone_Should_Throw() =>
        FluentActions.Invoking(() => Phone.Create("12-34")).Should().Throw<DomainException>();

    [Fact]
    public void Update_Unchanged_Should_Return_Same_Instance()
    {
        var p = Phone.Create("0612345678");
        p.Update("0612345678").Should().BeSameAs(p);
    }
}
