using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
    {
        public void Configure(EntityTypeBuilder<AlertRule> builder)
        {
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.StationId)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.RuleType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("threshold");

            builder.Property(x => x.ThresholdValue)
                .HasPrecision(14, 4)
                .IsRequired();

            builder.Property(x => x.DurationMin)
                .IsRequired(false);

            builder.Property(x => x.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("warning");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // NEW: IsGlobalDefault property
            builder.Property(x => x.IsGlobalDefault)
                .IsRequired()
                .HasDefaultValue(false);

            // Enum stored as string
            builder.Property(x => x.MinTierRequired)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

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
            builder.HasIndex(x => x.StationId)
                .HasDatabaseName("ix_alert_rules_station");

            builder.HasIndex(x => x.IsActive)
                .HasDatabaseName("ix_alert_rules_active");

            // Foreign key
            builder.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}