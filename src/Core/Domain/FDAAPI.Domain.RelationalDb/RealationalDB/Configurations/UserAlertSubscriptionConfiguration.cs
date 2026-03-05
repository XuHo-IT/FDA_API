using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class UserAlertSubscriptionConfiguration : IEntityTypeConfiguration<UserAlertSubscription>
    {
        public void Configure(EntityTypeBuilder<UserAlertSubscription> builder)
        {
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.UserId)
                .IsRequired();
            builder.Property(x => x.AreaId)
                .IsRequired(false);
            builder.Property(x => x.StationId)
                .IsRequired(false);
            builder.Property(x => x.MinSeverity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("warning");
            builder.Property(x => x.EnablePush)
                .IsRequired()
                .HasDefaultValue(true);
            builder.Property(x => x.EnableEmail)
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property(x => x.EnableSms)
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property(x => x.QuietHoursStart)
                .IsRequired(false);
            builder.Property(x => x.QuietHoursEnd)
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
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_user_alert_subscriptions_user");
            builder.HasIndex(x => new { x.UserId, x.StationId })
                .IsUnique()
                .HasDatabaseName("uq_user_alert_subscriptions_user_station");
            builder.HasIndex(x => x.StationId)
                .HasDatabaseName("ix_user_alert_subscriptions_station");
            builder.HasIndex(x => x.AreaId)
                .HasDatabaseName("ix_user_alert_subscriptions_area");

            // Foreign keys
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x => x.Area)
                .WithMany()
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}