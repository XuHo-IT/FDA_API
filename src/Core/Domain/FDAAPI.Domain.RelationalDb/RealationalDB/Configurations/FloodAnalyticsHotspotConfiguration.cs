using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodAnalyticsHotspotConfiguration : IEntityTypeConfiguration<FloodAnalyticsHotspot>
    {
        public void Configure(EntityTypeBuilder<FloodAnalyticsHotspot> builder)
        {
            builder.ToTable("FloodAnalyticsHotspots");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.AdministrativeAreaId).IsRequired();
            builder.Property(e => e.Score).IsRequired().HasColumnType("numeric(14,4)");
            builder.Property(e => e.Rank);
            builder.Property(e => e.PeriodStart).IsRequired();
            builder.Property(e => e.PeriodEnd).IsRequired();
            builder.Property(e => e.CalculatedAt).IsRequired();

            // Unique constraint on (administrative_area_id, period_start, period_end)
            builder.HasIndex(e => new { e.AdministrativeAreaId, e.PeriodStart, e.PeriodEnd })
                .IsUnique()
                .HasDatabaseName("uq_hotspot_area_period");

            // Indexes for query performance
            builder.HasIndex(e => new { e.Score, e.CalculatedAt })
                .IsDescending(false, true)
                .HasDatabaseName("ix_hotspot_score");
            builder.HasIndex(e => e.AdministrativeAreaId)
                .HasDatabaseName("ix_hotspot_area");

            // Relationship
            builder.HasOne(e => e.AdministrativeArea)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

