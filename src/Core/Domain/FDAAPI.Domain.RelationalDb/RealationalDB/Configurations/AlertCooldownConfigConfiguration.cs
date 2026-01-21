using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AlertCooldownConfigConfiguration : IEntityTypeConfiguration<AlertCooldownConfig>
    {
        public void Configure(EntityTypeBuilder<AlertCooldownConfig> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Severity)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.CooldownMinutes)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(e => e.MaxNotificationsPerHour)
                .IsRequired()
                .HasDefaultValue(6);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(e => e.Severity)
                .IsUnique()
                .HasDatabaseName("IX_AlertCooldownConfig_Severity");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_AlertCooldownConfig_IsActive");

            // Audit fields
            builder.Property(e => e.CreatedBy).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedBy).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired();

            // Seed default configurations
            var systemUserId = Guid.Empty;
            var now = DateTime.UtcNow;

            builder.HasData(
                new AlertCooldownConfig
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Severity = "info",
                    CooldownMinutes = 30,
                    MaxNotificationsPerHour = 2,
                    IsActive = true,
                    Description = "Low priority alerts - 30 min cooldown",
                    CreatedBy = systemUserId,
                    CreatedAt = now,
                    UpdatedBy = systemUserId,
                    UpdatedAt = now
                },
                new AlertCooldownConfig
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Severity = "caution",
                    CooldownMinutes = 20,
                    MaxNotificationsPerHour = 3,
                    IsActive = true,
                    Description = "Caution alerts - 20 min cooldown",
                    CreatedBy = systemUserId,
                    CreatedAt = now,
                    UpdatedBy = systemUserId,
                    UpdatedAt = now
                },
                new AlertCooldownConfig
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Severity = "warning",
                    CooldownMinutes = 10,
                    MaxNotificationsPerHour = 6,
                    IsActive = true,
                    Description = "Warning alerts - 10 min cooldown",
                    CreatedBy = systemUserId,
                    CreatedAt = now,
                    UpdatedBy = systemUserId,
                    UpdatedAt = now
                },
                new AlertCooldownConfig
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Severity = "critical",
                    CooldownMinutes = 5,
                    MaxNotificationsPerHour = 12,
                    IsActive = true,
                    Description = "Critical alerts - 5 min cooldown",
                    CreatedBy = systemUserId,
                    CreatedAt = now,
                    UpdatedBy = systemUserId,
                    UpdatedAt = now
                }
            );
        }
    }
}