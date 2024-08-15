public class OrderHeader
{
    public Guid Id { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderDetail> Details { get; set; } = [];
}