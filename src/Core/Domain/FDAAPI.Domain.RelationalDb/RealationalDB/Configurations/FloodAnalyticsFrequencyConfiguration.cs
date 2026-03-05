using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodAnalyticsFrequencyConfiguration : IEntityTypeConfiguration<FloodAnalyticsFrequency>
    {
        public void Configure(EntityTypeBuilder<FloodAnalyticsFrequency> builder)
        {
            builder.ToTable("FloodAnalyticsFrequency");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.AdministrativeAreaId).IsRequired();
            builder.Property(e => e.TimeBucket).IsRequired();
            builder.Property(e => e.BucketType).IsRequired().HasMaxLength(20);
            builder.Property(e => e.EventCount).HasDefaultValue(0);
            builder.Property(e => e.ExceedCount).HasDefaultValue(0);
            builder.Property(e => e.CalculatedAt).IsRequired();

            // Unique constraint on (administrative_area_id, time_bucket, bucket_type)
            builder.HasIndex(e => new { e.AdministrativeAreaId, e.TimeBucket, e.BucketType })
                .IsUnique()
                .HasDatabaseName("uq_frequency_area_bucket");

            // Indexes for query performance
            builder.HasIndex(e => new { e.AdministrativeAreaId, e.TimeBucket })
                .HasDatabaseName("ix_frequency_area_bucket");
            builder.HasIndex(e => new { e.BucketType, e.TimeBucket })
                .HasDatabaseName("ix_frequency_bucket_type");

            // Relationship
            builder.HasOne(e => e.AdministrativeArea)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

