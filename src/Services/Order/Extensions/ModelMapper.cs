public static class ModelMapper
{
    public static OrderDTO toDTO(this OrderHeader orderHeader) => new()
    {
        Customer = orderHeader.CustomerId,
        OrderDate = orderHeader.OrderDate,
        OrderHeaderId = orderHeader.Id,
        Status = orderHeader.Status,
        Details = orderHeader.Details
        .Select(x => new OrderProductDTO()
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
        })
    };
}