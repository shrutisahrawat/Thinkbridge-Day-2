using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OrderRefactor.Models;

namespace Quotes.Tests.Unit;

public class CreateOrderItemRequestValidationTests
{
    private static IList<ValidationResult> Validate(CreateOrderItemRequest item)
    {
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(item, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_ValidItem_ReturnsNoValidationErrors()
    {
        var item = new CreateOrderItemRequest { ProductName = "Compiler Manual", Price = 25.00m, Quantity = 2 };

        var results = Validate(item);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingProductName_ReturnsValidationError(string? productName)
    {
        var item = new CreateOrderItemRequest { ProductName = productName!, Price = 25.00m, Quantity = 2 };

        var results = Validate(item);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderItemRequest.ProductName)));
    }

    [Fact]
    public void Validate_ProductNameExceeding200Chars_ReturnsValidationError()
    {
        var item = new CreateOrderItemRequest { ProductName = new string('a', 201), Price = 25.00m, Quantity = 2 };

        var results = Validate(item);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderItemRequest.ProductName)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceZeroOrNegative_ReturnsValidationError(double price)
    {
        var item = new CreateOrderItemRequest { ProductName = "Item", Price = (decimal)price, Quantity = 1 };

        var results = Validate(item);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderItemRequest.Price)));
    }

    [Fact]
    public void Validate_PriceAtMinimumAllowedValue_ReturnsNoValidationErrors()
    {
        var item = new CreateOrderItemRequest { ProductName = "Item", Price = 0.01m, Quantity = 1 };

        var results = Validate(item);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_QuantityLessThanOne_ReturnsValidationError(int quantity)
    {
        var item = new CreateOrderItemRequest { ProductName = "Item", Price = 10m, Quantity = quantity };

        var results = Validate(item);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderItemRequest.Quantity)));
    }

    [Fact]
    public void Validate_QuantityOfOne_ReturnsNoValidationErrors()
    {
        var item = new CreateOrderItemRequest { ProductName = "Item", Price = 10m, Quantity = 1 };

        var results = Validate(item);

        results.Should().BeEmpty();
    }
}
