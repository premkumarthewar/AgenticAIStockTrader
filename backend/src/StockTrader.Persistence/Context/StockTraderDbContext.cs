using Microsoft.EntityFrameworkCore;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Context
{
    public class StockTraderDbContext(
    DbContextOptions<StockTraderDbContext> options) : DbContext(options)
    {
        public DbSet<TradingMemory> TradingMemories => Set<TradingMemory>();

        public DbSet<MemorySummary> MemorySummaries => Set<MemorySummary>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockTraderDbContext).Assembly);
        }
    }
}
