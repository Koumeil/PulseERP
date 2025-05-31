using FluentAssertions;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Tests.Domain;

public class MoneyTests
{
    [Theory]
    [InlineData(10, 5, 15)]
    [InlineData(0, 0, 0)]
    public void Addition_Should_Return_Correct_Value(decimal a, decimal b, decimal expected) =>
        (new Money(a) + new Money(b)).Value.Should().Be(expected);

    [Fact]
    public void Negative_Value_Should_Throw() =>
        FluentActions.Invoking(() => new Money(-1)).Should().Throw<ArgumentOutOfRangeException>();

    [Fact]
    public void Subtraction_Below_Zero_Should_Throw() =>
        FluentActions
            .Invoking(() => new Money(3) - new Money(5))
            .Should()
            .Throw<InvalidOperationException>();

    [Fact]
    public void Comparison_Operators_Should_Work()
    {
        var small = new Money(3);
        var big = new Money(8);

        (big > small).Should().BeTrue();
        (small < big).Should().BeTrue();
        (big >= small).Should().BeTrue();
        (small <= big).Should().BeTrue();
    }
}
