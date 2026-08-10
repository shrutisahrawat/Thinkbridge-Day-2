using System.ComponentModel.DataAnnotations;

namespace OrderRefactor.Models;

public class CreateOrderRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string? DiscountCode { get; set; }

    [Required, MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;
}

// Typed response — replaces the two duplicated anonymous objects (fixes smells #7 and #8)
public record OrderResponse(
    int OrderId,
    decimal Total,
    string Message,
    int PointsEarned,
    int ItemCount);