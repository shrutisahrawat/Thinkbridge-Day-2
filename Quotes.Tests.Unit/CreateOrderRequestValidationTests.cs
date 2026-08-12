using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OrderRefactor.Models;

namespace Quotes.Tests.Unit;

public class CreateOrderRequestValidationTests
{
    private static IList<ValidationResult> Validate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsNoValidationErrors()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Grace Hopper",
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingCustomerName_ReturnsValidationError(string? customerName)
    {
        var request = new CreateOrderRequest
        {
            CustomerName = customerName!,
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderRequest.CustomerName)));
    }

    [Fact]
    public void Validate_CustomerNameExceeding200Chars_ReturnsValidationError()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = new string('a', 201),
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderRequest.CustomerName)));
    }

    [Fact]
    public void Validate_CustomerNameExactly200Chars_ReturnsNoValidationErrors()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = new string('a', 200),
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_MissingOrInvalidEmail_ReturnsValidationError(string email)
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Grace Hopper",
            CustomerEmail = email,
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderRequest.CustomerEmail)));
    }

    [Fact]
    public void Validate_EmptyItemsList_ReturnsValidationError()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Grace Hopper",
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest>()
        };

        var results = Validate(request);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateOrderRequest.Items)));
    }

    [Fact]
    public void Validate_NullDiscountCode_ReturnsNoValidationErrors()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "Grace Hopper",
            CustomerEmail = "grace@example.com",
            DiscountCode = null,
            Items = new List<CreateOrderItemRequest> { new() { ProductName = "Item", Price = 10m, Quantity = 1 } }
        };

        var results = Validate(request);

        results.Should().BeEmpty();
    }
}
