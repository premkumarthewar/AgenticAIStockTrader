using Microsoft.EntityFrameworkCore;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Context
{
    public class StockTraderDbContext(
    DbContextOptions<StockTraderDbContext> options) : DbContext(options)
    {
        public DbSet<TradingMemory> TradingMemories => Set<TradingMemory>();

        public DbSet<MemorySummary> MemorySummaries => Set<MemorySummary>();

        public DbSet<PaperPortfolio> PaperPortfolios => Set<PaperPortfolio>();

        public DbSet<PaperPosition> PaperPositions => Set<PaperPosition>();

        public DbSet<PaperTrade> PaperTrades => Set<PaperTrade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockTraderDbContext).Assembly);
        }
    }
}
