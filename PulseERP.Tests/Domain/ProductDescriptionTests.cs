using FluentAssertions;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Tests.Domain;

public class ProductDescriptionTests
{
    [Fact]
    public void Null_Description_Is_Allowed() =>
         ProductDescription.Create(null).Value.Should().BeNull();

    [Fact]
    public void Too_Long_Should_Fail() =>
        FluentActions
            .Invoking(() =>  ProductDescription.Create(new string('A', 1001)))
            .Should()
            .Throw<DomainException>();
}
