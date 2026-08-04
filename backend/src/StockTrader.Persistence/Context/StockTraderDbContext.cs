using Microsoft.EntityFrameworkCore;

namespace StockTrader.Persistence.Context
{
    public class StockTraderDbContext(
    DbContextOptions<StockTraderDbContext> options) : DbContext(options)
    {
    }
}
