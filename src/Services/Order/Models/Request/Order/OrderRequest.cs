public class OrderRequest
{
    public Guid UserId { get; set; }
    public IEnumerable<OrderRequestProduct> OrderProducts { get; set; } = null!;
}

public class OrderRequestProduct
{
    public Guid ProductId { get; set; }
    public long Quantity { get; set; }
}