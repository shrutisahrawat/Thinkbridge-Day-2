using OrderRefactor.Models;

namespace OrderRefactor.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct);
}