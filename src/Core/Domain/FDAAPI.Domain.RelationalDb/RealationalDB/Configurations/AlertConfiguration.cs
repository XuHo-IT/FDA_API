using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.AlertRuleId)
                .IsRequired();
            builder.Property(x => x.StationId)
                .IsRequired();
            builder.Property(x => x.TriggeredAt)
                .IsRequired();
            builder.Property(x => x.ResolvedAt)
                .IsRequired(false);
            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("open");
            builder.Property(x => x.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("info");
            builder.Property(x => x.Priority)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
            builder.Property(x => x.CurrentValue)
                .HasPrecision(14, 4);
            builder.Property(x => x.Message)
                .IsRequired()
                .HasColumnType("text");

            // ADD: Notification tracking fields
            builder.Property(x => x.NotificationSent)
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property(x => x.NotificationCount)
                .IsRequired()
                .HasDefaultValue(0);
            builder.Property(x => x.LastNotificationAt)
                .IsRequired(false);

            // Audit fields
            builder.Property(x => x.CreatedBy)
                .IsRequired();
            builder.Property(x => x.CreatedAt)
                .IsRequired();
            builder.Property(x => x.UpdatedBy)
                .IsRequired();
            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => new { x.StationId, x.TriggeredAt })
                .HasDatabaseName("ix_alerts_station_triggered");
            builder.HasIndex(x => x.Status)
                .HasDatabaseName("ix_alerts_status");
            builder.HasIndex(x => x.Severity)
                .HasDatabaseName("ix_alerts_severity");
            builder.HasIndex(x => new { x.NotificationSent, x.Status })
                .HasDatabaseName("ix_alerts_notification_status");

            // Foreign keys
            builder.HasOne(x => x.AlertRule)
                .WithMany(ar => ar.Alerts)
                .HasForeignKey(x => x.AlertRuleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}