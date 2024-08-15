
using Microsoft.EntityFrameworkCore;

public class OrderService : ServiceBase, IOrderService
{
    private readonly OrderDbContext _dbContext;
    private readonly IMessageProducer _messageProducer;
    public OrderService(OrderDbContext dbContext, IMessageProducer messageProducer, IConfiguration configuration) : base(configuration)
    {
        _dbContext = dbContext;
        _messageProducer = messageProducer;
    }

    public async Task<OrderDTO> NewOrder(OrderRequest orderRequest, CancellationToken cancellationToken)
    {
        var order = await SaveOrder(orderRequest, cancellationToken);

        _messageProducer.SendMessage(order, GetQueueName("StockUpdate"));

        return order;
    }

    private async Task<OrderDTO> SaveOrder(OrderRequest orderRequest, CancellationToken cancellationToken)
    {
        OrderHeader header = new()
        {
            OrderDate = DateTimeOffset.Now,
            CustomerId = orderRequest.UserId,
            Status = OrderStatus.Pending
        };

        foreach (var orderProduct in orderRequest.OrderProducts)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = orderProduct.ProductId,
                Quantity = orderProduct.Quantity,
                OrderHeaderId = header.Id
            };
            header.Details.Add(orderDetail);
        }

        await _dbContext.OrderHeaders.AddAsync(header, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return header.toDTO();
    }

    public async Task ApproveOrder(OrderDTO orderDTO, CancellationToken cancellationToken)
    {
        await UpdateOrderStatus(orderDTO.OrderHeaderId, OrderStatus.Approved, cancellationToken);

        _messageProducer.SendMessage(
            new Notification("Tuðkan", "Your order approved!", $"Dear Tuðkan, your order is just approved. It will be shipped soon. OrderId: {orderDTO.OrderHeaderId}"), 
            GetQueueName("Notify"));
    }
    

    public async Task CancelOrder(OrderDTO orderDTO, CancellationToken cancellationToken)
    {
        await UpdateOrderStatus(orderDTO.OrderHeaderId, OrderStatus.Canceled, cancellationToken);

        _messageProducer.SendMessage(
            new Notification("Tuðkan", "Your order canceled!", $"Dear Tuðkan, your order is canceled unfortunately. OrderId: {orderDTO.OrderHeaderId}"),
            GetQueueName("Notify"));
    }
    
    public async Task<OrderDTO?> GetOrder(Guid orderHeaderId, CancellationToken cancellationToken)
    {
        var orderHeader = _dbContext.OrderHeaders
            .AsNoTracking()
            .Where(x => x.Id == orderHeaderId);

        var orderDetails = _dbContext.OrderDetails
            .AsNoTracking();

        var order = await orderHeader
            .Join(
                orderDetails,
                h => h.Id,
                d => d.OrderHeaderId,
                (h, d) => new
                {
                    Header = h,
                    Detail = d
                })
            .GroupBy(
                joined => joined.Header.Id,
                (key, grouped) => new OrderDTO
                {
                    OrderHeaderId = key,
                    OrderDate = grouped.First().Header.OrderDate,
                    Customer = grouped.First().Header.CustomerId,
                    Status = grouped.First().Header.Status,
                    Details = grouped.Select(g => new OrderProductDTO
                    {
                        ProductId = g.Detail.ProductId,
                        Quantity = g.Detail.Quantity,
                    }).ToList()
                }
            )
            .FirstOrDefaultAsync(cancellationToken);

        return order;
    }

    private async Task UpdateOrderStatus(Guid orderHeaderId, OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await _dbContext.OrderHeaders
                    .FindAsync(orderHeaderId, cancellationToken);

        ArgumentNullException.ThrowIfNull(order);

        order.Status = status;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}