using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.RealationalDB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<WaterLevel> WaterLevels { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<OtpCode> OtpCodes { get; set; } = null!;
        public DbSet<UserOAuthProvider> UserOAuthProviders { get; set; } = null!;
        public DbSet<Station> Stations { get; set; } = null!;
        public DbSet<UserPreference> UserPreferences { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WaterLevel>()
                .Property(w => w.Id)
                .ValueGeneratedOnAdd();

            // Existing WaterLevel config
            modelBuilder.Entity<WaterLevel>()
                .Property(w => w.Id)
                .ValueGeneratedOnAdd();

            // Users table configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.Property(e => e.Status).HasDefaultValue("ACTIVE");
                entity.Property(e => e.Provider).HasDefaultValue("local");
            });

            // Roles table configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // UserRoles table configuration (many-to-many)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            });

            // RefreshTokens table configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            // OtpCodes table configuration
            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber);
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
                entity.Property(e => e.AttemptCount).HasDefaultValue(0);
            });

            // Seed initial Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Code = "ADMIN",
                    Name = "Administrator"
                },
                new Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Code = "AUTHORITY",
                    Name = "Authority Officer"
                },
                new Role
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Code = "USER",
                    Name = "Citizen User"
                },
                new Role
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Code = "SUPERADMIN",
                    Name = "Super Administrator"
                }
            );

            // UserOAuthProviders table configuration
            modelBuilder.Entity<UserOAuthProvider>(entity =>
            {
                entity.ToTable("UserOAuthProviders");
                entity.HasKey(e => e.Id);

                // Unique constraint: One provider per user
                entity.HasIndex(e => new { e.UserId, e.Provider })
                    .IsUnique()
                    .HasDatabaseName("uq_user_oauth_provider");

                // Unique constraint: One provider user ID per provider
                entity.HasIndex(e => new { e.Provider, e.ProviderUserId })
                    .IsUnique()
                    .HasDatabaseName("uq_provider_user_id");

                // Foreign key relationship with cascade delete
                entity.HasOne(e => e.User)
                    .WithMany(u => u.OAuthProviders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Required fields
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProviderUserId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.DisplayName).HasMaxLength(255);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            });

            // UserPreference configuration
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.PreferenceKey)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.PreferenceValue)
                    .HasColumnType("jsonb")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired();

                // Unique constraint: one preference per user per key
                entity.HasIndex(e => new { e.UserId, e.PreferenceKey })
                    .IsUnique()
                    .HasDatabaseName("uq_user_preference");

                // Indexes for performance
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_user_preferences_user");

                entity.HasIndex(e => e.PreferenceKey)
                    .HasDatabaseName("ix_user_preferences_key");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserPreferences)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}






