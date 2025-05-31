using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Tests.Domain;

public class ProductDescriptionTests
{
    [Fact]
    public void Null_Description_Is_Allowed() =>
        new ProductDescription(null).Value.Should().BeNull();

    [Fact]
    public void Too_Long_Should_Fail() =>
        FluentActions
            .Invoking(() => new ProductDescription(new string('A', 1001)))
            .Should()
            .Throw<DomainException>();
}
