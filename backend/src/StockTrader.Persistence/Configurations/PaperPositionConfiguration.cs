using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class PaperPositionConfiguration : IEntityTypeConfiguration<PaperPosition>
{
    public void Configure(EntityTypeBuilder<PaperPosition> builder)
    {
        builder.ToTable("PaperPositions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(20).IsRequired();

        builder.Property(x => x.Quantity).HasPrecision(18, 6);

        builder.Property(x => x.AveragePrice).HasPrecision(18, 2);

        builder.Property(x => x.LastUpdatedOnUtc).IsRequired();

        builder.HasIndex(x => new
        {
            x.PaperPortfolioId,
            x.Symbol
        })
        .IsUnique();
    }
}
