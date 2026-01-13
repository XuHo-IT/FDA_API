using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class UserOAuthProviderConfiguration : IEntityTypeConfiguration<UserOAuthProvider>
    {
        public void Configure(EntityTypeBuilder<UserOAuthProvider> builder)
        {
            builder.ToTable("UserOAuthProviders");
            
            builder.HasKey(e => e.Id);

            // Unique constraint: One provider per user
            builder.HasIndex(e => new { e.UserId, e.Provider })
                .IsUnique()
                .HasDatabaseName("uq_user_oauth_provider");

            // Unique constraint: One provider user ID per provider
            builder.HasIndex(e => new { e.Provider, e.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("uq_provider_user_id");

            // Foreign key relationship with cascade delete
            builder.HasOne(e => e.User)
                .WithMany(u => u.OAuthProviders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Required fields
            builder.Property(e => e.Provider)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(e => e.ProviderUserId)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.Email)
                .HasMaxLength(255);
            
            builder.Property(e => e.DisplayName)
                .HasMaxLength(255);
            
            builder.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(500);
        }
    }
}
