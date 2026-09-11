using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class TradingMemoryConfiguration : IEntityTypeConfiguration<TradingMemory>
{
    public void Configure(EntityTypeBuilder<TradingMemory> builder)
    {
        builder.ToTable("TradingMemories");

        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Symbol).HasMaxLength(20).IsRequired();

        builder.Property(x => x.SourceAgent).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => x.Symbol);

        builder.HasIndex(x => x.CreatedOnUtc);
    }
}
