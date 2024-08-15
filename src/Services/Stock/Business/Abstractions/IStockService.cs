public interface IStockService
{
    public Task UpdateStock(OrderDTO order, CancellationToken cancellationToken);
    public Task<Guid> Add(ProductStockDTO productStockDTO, CancellationToken cancellationToken);
    public Task<ProductStockDTO?> Get(Guid productId, CancellationToken cancellationToken);
}