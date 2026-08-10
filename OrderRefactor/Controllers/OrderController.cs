using Microsoft.AspNetCore.Mvc;
using OrderRefactor.Models;
using OrderRefactor.Services;

namespace OrderRefactor.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var response = await _orderService.CreateOrderAsync(request, ct);
        return CreatedAtAction(nameof(CreateOrder), new { id = response.OrderId }, response);
    }
}