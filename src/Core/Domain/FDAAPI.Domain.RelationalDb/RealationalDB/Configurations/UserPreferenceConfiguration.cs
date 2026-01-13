using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.PreferenceKey)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.PreferenceValue)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Unique constraint: one preference per user per key
            builder.HasIndex(e => new { e.UserId, e.PreferenceKey })
                .IsUnique()
                .HasDatabaseName("uq_user_preference");

            // Indexes for performance
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_user_preferences_user");

            builder.HasIndex(e => e.PreferenceKey)
                .HasDatabaseName("ix_user_preferences_key");

            // Foreign key relationship
            builder.HasOne(e => e.User)
                .WithMany(u => u.UserPreferences)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
