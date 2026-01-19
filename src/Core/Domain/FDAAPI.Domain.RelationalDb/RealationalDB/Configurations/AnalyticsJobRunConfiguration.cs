using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AnalyticsJobRunConfiguration : IEntityTypeConfiguration<AnalyticsJobRun>
    {
        public void Configure(EntityTypeBuilder<AnalyticsJobRun> builder)
        {
            builder.ToTable("AnalyticsJobRuns");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.JobId).IsRequired();
            builder.Property(e => e.StartedAt).IsRequired();
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.ErrorMessage).HasColumnType("text");
            builder.Property(e => e.RecordsProcessed).HasDefaultValue(0);
            builder.Property(e => e.RecordsCreated).HasDefaultValue(0);
            builder.Property(e => e.CreatedAt).IsRequired();

            // Indexes for query performance
            builder.HasIndex(e => new { e.JobId, e.StartedAt })
                .IsDescending(false, true)
                .HasDatabaseName("ix_job_runs_job");
            builder.HasIndex(e => new { e.Status, e.StartedAt })
                .IsDescending(false, true)
                .HasDatabaseName("ix_job_runs_status");

            // Relationship
            builder.HasOne(e => e.Job)
                .WithMany()
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

