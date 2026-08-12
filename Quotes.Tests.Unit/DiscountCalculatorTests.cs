using FluentAssertions;
using OrderRefactor.Services;

namespace Quotes.Tests.Unit;

public class DiscountCalculatorTests
{
    [Fact]
    public void GetDiscountPercent_NullCodeNotVip_ReturnsZero()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent(null, isVip: false);

        result.Should().Be(0m);
    }

    [Fact]
    public void GetDiscountPercent_WhitespaceCodeNotVip_ReturnsZero()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent("   ", isVip: false);

        result.Should().Be(0m);
    }

    [Fact]
    public void GetDiscountPercent_UnknownCodeNotVip_ReturnsZero()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent("NOTAREALCODE", isVip: false);

        result.Should().Be(0m);
    }

    [Theory]
    [InlineData("SAVE10", 0.10)]
    [InlineData("SAVE20", 0.20)]
    [InlineData("VIP", 0.30)]
    public void GetDiscountPercent_KnownCodeNotVip_ReturnsCodePercent(string code, double expected)
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent(code, isVip: false);

        result.Should().Be((decimal)expected);
    }

    [Fact]
    public void GetDiscountPercent_LowercaseKnownCode_ReturnsZero()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent("save10", isVip: false);

        result.Should().Be(0m);
    }

    [Fact]
    public void GetDiscountPercent_VipWithNoCode_ReturnsVipBonusOnly()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent(null, isVip: true);

        result.Should().Be(0.05m);
    }

    [Fact]
    public void GetDiscountPercent_VipWithUnknownCode_ReturnsVipBonusOnly()
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent("NOTAREALCODE", isVip: true);

        result.Should().Be(0.05m);
    }

    [Theory]
    [InlineData("SAVE10", 0.15)]
    [InlineData("SAVE20", 0.25)]
    [InlineData("VIP", 0.35)]
    public void GetDiscountPercent_VipWithKnownCode_StacksCodeAndVipBonus(string code, double expected)
    {
        var calculator = new DiscountCalculator();

        var result = calculator.GetDiscountPercent(code, isVip: true);

        result.Should().Be((decimal)expected);
    }
}
