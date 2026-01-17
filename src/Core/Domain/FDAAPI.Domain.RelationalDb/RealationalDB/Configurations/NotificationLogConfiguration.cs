using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
    {
        public void Configure(EntityTypeBuilder<NotificationLog> builder)
        {
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.AlertId)
                .IsRequired();

            // Enums stored as strings
            builder.Property(x => x.Channel)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Destination)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.Priority)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.MaxRetries)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            builder.Property(x => x.SentAt)
                .IsRequired(false);

            builder.Property(x => x.DeliveredAt)
                .IsRequired(false);

            builder.Property(x => x.ErrorMessage)
                .IsRequired(false)
                .HasMaxLength(1000);

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
            builder.HasIndex(x => new { x.UserId, x.CreatedAt })
                .HasDatabaseName("ix_notification_logs_user_time");

            builder.HasIndex(x => x.AlertId)
                .HasDatabaseName("ix_notification_logs_alert");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("ix_notification_logs_status");

            builder.HasIndex(x => new { x.Status, x.RetryCount })
                .HasDatabaseName("ix_notification_logs_retry");

            // Foreign keys
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Alert)
                .WithMany(a => a.NotificationLogs)
                .HasForeignKey(x => x.AlertId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}