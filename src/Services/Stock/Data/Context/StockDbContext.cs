

using Microsoft.EntityFrameworkCore;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    public DbSet<ProductStock> ProductStocks { get; set; }
}