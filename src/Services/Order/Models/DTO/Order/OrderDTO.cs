public class OrderDTO
{
    public Guid OrderHeaderId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public Guid Customer { get; set; }
    public OrderStatus Status { get; set; }
    public IEnumerable<OrderProductDTO> Details { get; set; } = null!;
}

public class OrderProductDTO
{
    public Guid ProductId { get; set; }
    public long Quantity { get; set; }
}