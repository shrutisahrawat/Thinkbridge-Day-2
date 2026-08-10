namespace OrderRefactor.Services;

public interface IDiscountCalculator
{
    decimal GetDiscountPercent(string? discountCode, bool isVip);
}

public class DiscountCalculator : IDiscountCalculator
{
    private static readonly Dictionary<string, decimal> KnownCodes = new()
    {
        ["SAVE10"] = 0.10m,
        ["SAVE20"] = 0.20m,
        ["VIP"] = 0.30m
    };

    private const decimal VipBonusPercent = 0.05m;

    public decimal GetDiscountPercent(string? discountCode, bool isVip)
    {
        decimal discount = 0m;

        if (!string.IsNullOrWhiteSpace(discountCode) && KnownCodes.TryGetValue(discountCode, out var codeDiscount))
        {
            discount = codeDiscount;
        }

        if (isVip)
        {
            discount += VipBonusPercent;
        }

        return discount;
    }
}