using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Tests.Domain;

public class ProductNameTests
{
    [Fact]
    public void Valid_Name_Should_Create() => ProductName.Create("Valid").Value.Should().Be("Valid");

    [Fact]
    public void Too_Long_Should_Fail() =>
        FluentActions
            .Invoking(() => ProductName.Create(new string('A', 201)))
            .Should()
            .Throw<DomainException>();
}
