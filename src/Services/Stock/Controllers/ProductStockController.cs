using Microsoft.AspNetCore.Mvc;

namespace Stock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductStockController : ControllerBase
{
    private readonly IStockService _stockService;

    public ProductStockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid productId, CancellationToken cancellationToken)
    {
        var existStock = await _stockService.Get(productId, cancellationToken);

        if (existStock == null)
            return NoContent();
        else
            return Ok(existStock);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ProductStockDTO productStock, CancellationToken cancellationToken)
    {
        var stockId = await _stockService.Add(productStock, cancellationToken);
        return Ok(stockId);
    }
}