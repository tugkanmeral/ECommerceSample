public interface IOrderService
{
    Task<OrderDTO> NewOrder(OrderRequest orderRequest, CancellationToken cancellationToken);
    Task ApproveOrder(OrderDTO order,  CancellationToken cancellationToken);
    Task CancelOrder(OrderDTO order,  CancellationToken cancellationToken);
    Task<OrderDTO?> GetOrder(Guid orderHeaderId, CancellationToken cancellationToken);
}