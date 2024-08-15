public class OrderDetail
{
    public Guid Id { get; set; }
    public Guid OrderHeaderId { get; set; }
    public Guid ProductId { get; set; }
    public long Quantity { get; set; }
}