using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class PaperTradeConfiguration : IEntityTypeConfiguration<PaperTrade>
{
    public void Configure(EntityTypeBuilder<PaperTrade> builder)
    {
        builder.ToTable("PaperTrades");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Action).HasMaxLength(10).IsRequired();

        builder.Property(x => x.Quantity).HasPrecision(18, 6);

        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.Property(x => x.TotalValue).HasPrecision(18, 2);

        builder.Property(x => x.ExecutedAtUtc).IsRequired();

        builder.HasIndex(x => new
        {
            x.PaperPortfolioId,
            x.ExecutedAtUtc
        });
    }
}
