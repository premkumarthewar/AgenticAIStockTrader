using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class TradeApprovalConfiguration : IEntityTypeConfiguration<TradeApproval>
{
    public void Configure(EntityTypeBuilder<TradeApproval> builder)
    {
        builder.ToTable("TradeApprovals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(20).IsRequired();

        builder.Property(x => x.Action).HasMaxLength(10).IsRequired();

        builder.Property(x => x.Quantity).HasPrecision(18, 6);

        builder.Property(x => x.TargetPrice).HasPrecision(18, 2);

        builder.Property(x => x.Confidence).HasPrecision(5, 2);

        builder.Property(x => x.RiskLevel).HasMaxLength(20).IsRequired();

        builder.Property(x => x.Reasoning).HasMaxLength(4000).IsRequired();

        builder.Property(x => x.BrokerProvider).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RequestedOnUtc).IsRequired();

        builder.Property(x => x.DecisionNotes).HasMaxLength(2000);

        builder.Property(x => x.BrokerOrderId).HasMaxLength(100);

        builder.Property(x => x.ExecutionError).HasMaxLength(2000);

        builder.HasIndex(x => new { x.Status, x.RequestedOnUtc });
    }
}
