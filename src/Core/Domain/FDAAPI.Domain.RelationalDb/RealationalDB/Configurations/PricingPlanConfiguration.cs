using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class PricingPlanConfiguration : IEntityTypeConfiguration<PricingPlan>
    {
        public void Configure(EntityTypeBuilder<PricingPlan> builder)
        {
            builder.ToTable("PricingPlans");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Code).IsUnique();

            builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Tier).IsRequired();

            // Seed 3 default plans
            builder.HasData(
                new PricingPlan
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Code = "FREE",
                    Name = "Free Plan",
                    Description = "Basic flood alerts with push and email notifications",
                    PriceMonth = 0,
                    PriceYear = 0,
                    Tier = SubscriptionTier.Free,
                    IsActive = true,
                    SortOrder = 1,
                    CreatedBy = Guid.Empty,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedBy = Guid.Empty,
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new PricingPlan
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Code = "PREMIUM",
                    Name = "Premium Plan",
                    Description = "Priority alerts with SMS, Email, and Push - faster delivery",
                    PriceMonth = 9.99m,
                    PriceYear = 99.99m,
                    Tier = SubscriptionTier.Premium,
                    IsActive = true,
                    SortOrder = 2,
                    CreatedBy = Guid.Empty,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedBy = Guid.Empty,
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new PricingPlan
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Code = "MONITOR",
                    Name = "Monitor Access",
                    Description = "For monitoring agencies - all channels, immediate delivery",
                    PriceMonth = 0,
                    PriceYear = 0,
                    Tier = SubscriptionTier.Monitor,
                    IsActive = true,
                    SortOrder = 3,
                    CreatedBy = Guid.Empty,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedBy = Guid.Empty,
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
