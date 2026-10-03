using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Identity;

namespace StockTrader.Persistence.Context
{
    // IdentityDbContext<...> adds the Users/Roles/UserClaims/UserLogins/UserTokens/
    // UserRoles/RoleClaims DbSets and table mappings on top of the app's own DbSets
    // below - ApplicationUser.Id is a Guid, matched by the Guid key parameter here, so
    // every foreign key into it (e.g. PaperPortfolio.UserId) is a plain Guid too.
    public class StockTraderDbContext(
    DbContextOptions<StockTraderDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<TradingMemory> TradingMemories => Set<TradingMemory>();

        public DbSet<MemorySummary> MemorySummaries => Set<MemorySummary>();

        public DbSet<PaperPortfolio> PaperPortfolios => Set<PaperPortfolio>();

        public DbSet<PaperPosition> PaperPositions => Set<PaperPosition>();

        public DbSet<PaperTrade> PaperTrades => Set<PaperTrade>();

        public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();

        public DbSet<MonitoringAlert> MonitoringAlerts => Set<MonitoringAlert>();

        public DbSet<TradeApproval> TradeApprovals => Set<TradeApproval>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockTraderDbContext).Assembly);
        }
    }
}
