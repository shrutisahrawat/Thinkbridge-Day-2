using OrderRefactor.Services;
using Xunit;

namespace OrderRefactor.Tests;

public class DiscountCalculatorTests
{
    private readonly IDiscountCalculator _calculator = new DiscountCalculator();

    [Fact]
    public void GetDiscountPercent_WithNoCodeAndNotVip_ReturnsZero()
    {
        var result = _calculator.GetDiscountPercent(null, isVip: false);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetDiscountPercent_WithKnownCode_ReturnsCorrectPercent()
    {
        var result = _calculator.GetDiscountPercent("SAVE20", isVip: false);
        Assert.Equal(0.20m, result);
    }

    [Fact]
    public void GetDiscountPercent_VipWithCode_StacksBonus()
    {
        var result = _calculator.GetDiscountPercent("SAVE10", isVip: true);
        // 0.10 (code) + 0.05 (vip bonus) = 0.15
        Assert.Equal(0.15m, result);
    }
}