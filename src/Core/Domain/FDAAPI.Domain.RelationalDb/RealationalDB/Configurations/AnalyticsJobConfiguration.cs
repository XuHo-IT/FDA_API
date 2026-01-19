using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AnalyticsJobConfiguration : IEntityTypeConfiguration<AnalyticsJob>
    {
        public void Configure(EntityTypeBuilder<AnalyticsJob> builder)
        {
            builder.ToTable("AnalyticsJobs");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.JobType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Schedule).HasMaxLength(100);
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on job_type
            builder.HasIndex(e => e.JobType)
                .IsUnique()
                .HasDatabaseName("uq_job_type");

            // Indexes for query performance
            builder.HasIndex(e => e.JobType)
                .HasDatabaseName("ix_jobs_type");
            builder.HasIndex(e => new { e.IsActive, e.NextRunAt })
                .HasDatabaseName("ix_jobs_active");
        }
    }
}

