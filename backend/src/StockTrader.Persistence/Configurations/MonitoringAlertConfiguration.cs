using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockTrader.Domain.Entities;

namespace StockTrader.Persistence.Configurations;

public sealed class MonitoringAlertConfiguration : IEntityTypeConfiguration<MonitoringAlert>
{
    public void Configure(EntityTypeBuilder<MonitoringAlert> builder)
    {
        builder.ToTable("MonitoringAlerts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(16);

        builder.Property(x => x.AlertType).IsRequired().HasMaxLength(32);

        builder.Property(x => x.Message).IsRequired().HasMaxLength(512);

        builder.Property(x => x.TriggeredAtUtc).IsRequired();

        builder.HasIndex(x => x.TriggeredAtUtc);
    }
}
