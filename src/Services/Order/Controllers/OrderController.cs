using Microsoft.AspNetCore.Mvc;

namespace Order.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(OrderRequest orderRequest, CancellationToken cancellationToken)
    {
        var order = await _orderService.NewOrder(orderRequest, cancellationToken);
        return Created(nameof(Post), order.OrderHeaderId);
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrder(orderId, cancellationToken);
        return Ok(order);
    }
}
