using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class PaperPortfolioConfiguration : IEntityTypeConfiguration<PaperPortfolio>
{
    public void Configure(EntityTypeBuilder<PaperPortfolio> builder)
    {
        builder.ToTable("PaperPortfolios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InitialCapital).HasPrecision(18, 2);

        builder.Property(x => x.CashBalance).HasPrecision(18, 2);

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.Property(x => x.LastUpdatedOnUtc).IsRequired();

        builder.HasMany(x => x.Positions).WithOne(x => x.PaperPortfolio).HasForeignKey(x => x.PaperPortfolioId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Trades).WithOne(x => x.PaperPortfolio).HasForeignKey(x => x.PaperPortfolioId).OnDelete(DeleteBehavior.Cascade);
    }
}
