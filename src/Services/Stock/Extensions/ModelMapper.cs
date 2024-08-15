public static class ModelMapper
{
    public static ProductStockDTO toDTO(this ProductStock productStock) => new()
    {
        ProductId = productStock.ProductId,
        Quantity = productStock.Quantity,
    };
}