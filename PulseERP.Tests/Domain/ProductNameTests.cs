using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Tests.Domain;

public class ProductNameTests
{
    [Fact]
    public void Valid_Name_Should_Create() => new ProductName("Valid").Value.Should().Be("Valid");

    [Fact]
    public void Too_Long_Should_Fail() =>
        FluentActions
            .Invoking(() => new ProductName(new string('A', 201)))
            .Should()
            .Throw<DomainException>();
}
