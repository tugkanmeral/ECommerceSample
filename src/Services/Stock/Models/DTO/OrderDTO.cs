public class OrderDTO
{
    public Guid OrderHeaderId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public Guid Customer { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderDetailDTO> Details { get; set; } = [];
}