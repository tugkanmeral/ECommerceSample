using Microsoft.EntityFrameworkCore;

public class StockService : ServiceBase, IStockService
{
    private readonly StockDbContext _dbContext;
    private readonly ILogger<StockService> _logger;
    private readonly IMessageProducer _messageProducer;
    public StockService(StockDbContext dbContext, ILogger<StockService> logger, IMessageProducer messageProducer, IConfiguration configuration) : base(configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _messageProducer = messageProducer;
    }

    public async Task UpdateStock(OrderDTO order, CancellationToken cancellationToken)
    {
        using var transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var productQuantities = order.Details
            .GroupBy(detail => detail.ProductId)
            .Select(x =>
                new ProductQuantityDTO(
                    x.Key,
                    x.Sum(detail => detail.Quantity)
                    )
                );

            foreach (var productQty in productQuantities)
            {
                var enoughStock = await _dbContext.ProductStocks
                    .SingleOrDefaultAsync(x => x.ProductId == productQty.ProductId && x.Quantity >= productQty.Quantity, cancellationToken);

                if (enoughStock == null)
                    throw new Exception($"Insufficient stock for the product with id {productQty.ProductId}");

                enoughStock.Quantity -= productQty.Quantity;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Stock updated. productId: {enoughStock.ProductId}, current product stock: {enoughStock.Quantity}");
            }

            await transaction.CommitAsync(cancellationToken);

            _messageProducer.SendMessage(order, GetQueueName("OrderApprove"));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            await transaction.RollbackAsync(cancellationToken);

            _messageProducer.SendMessage(order, GetQueueName("OrderCancel"));
        }
    }

    public async Task<Guid> Add(ProductStockDTO productStockDTO, CancellationToken cancellationToken)
    {
        var existProductStock = await _dbContext.ProductStocks
            .SingleOrDefaultAsync(x => x.ProductId == productStockDTO.ProductId, cancellationToken);

        if (existProductStock != null)
        {
            existProductStock.Quantity += productStockDTO.Quantity;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existProductStock.Id;
        }

        ProductStock productStock = new()
        {
            ProductId = productStockDTO.ProductId,
            Quantity = productStockDTO.Quantity,
        };

        await _dbContext.ProductStocks.AddAsync(productStock, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return productStock.ProductId;
    }

    public async Task<ProductStockDTO?> Get(Guid productId, CancellationToken cancellationToken)
    {
        var existProductStock = await _dbContext.ProductStocks
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

        return existProductStock?.toDTO();
    }
}