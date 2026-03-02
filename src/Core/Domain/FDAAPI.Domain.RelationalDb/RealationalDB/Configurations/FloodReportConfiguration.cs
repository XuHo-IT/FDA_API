using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodReportConfiguration : IEntityTypeConfiguration<FloodReport>
    {
        public void Configure(EntityTypeBuilder<FloodReport> builder)
        {
            builder.ToTable("FloodReports");

            builder.HasKey(x => x.Id);

            // Indexes
            builder.HasIndex(x => x.ReporterUserId)
                .HasDatabaseName("ix_flood_reports_reporter");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("ix_flood_reports_status");

            builder.HasIndex(x => x.Priority)
                .HasDatabaseName("ix_flood_reports_priority");

            builder.HasIndex(x => x.TrustScore)
                .HasDatabaseName("ix_flood_reports_trust_score");

            builder.HasIndex(x => new { x.Latitude, x.Longitude })
                .HasDatabaseName("ix_flood_reports_location");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("ix_flood_reports_created");

            // Properties
            builder.Property(x => x.Latitude)
                .IsRequired()
                .HasPrecision(9, 6);

            builder.Property(x => x.Longitude)
                .IsRequired()
                .HasPrecision(9, 6);

            builder.Property(x => x.Address)
                .HasColumnType("text");

            builder.Property(x => x.Description)
                .HasColumnType("text");

            builder.Property(x => x.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("medium")
                .HasComment("low | medium | high");

            builder.Property(x => x.TrustScore)
                .IsRequired()
                .HasDefaultValue(50)
                .HasComment("0-100");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("published")
                .HasComment("published | hidden | escalated");

            builder.Property(x => x.ConfidenceLevel)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("medium")
                .HasComment("low | medium | high");

            builder.Property(x => x.Priority)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("normal")
                .HasComment("normal | high | critical");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Constraints
            builder.HasCheckConstraint("chk_trust_score", "trust_score >= 0 AND trust_score <= 100");
            builder.HasCheckConstraint("chk_severity", "severity IN ('low', 'medium', 'high')");
            builder.HasCheckConstraint("chk_status", "status IN ('published', 'hidden', 'escalated')");
            builder.HasCheckConstraint("chk_priority", "priority IN ('normal', 'high', 'critical')");

            // Relationships
            builder.HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Media)
                .WithOne(m => m.FloodReport)
                .HasForeignKey(m => m.FloodReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Votes)
                .WithOne(v => v.FloodReport)
                .HasForeignKey(v => v.FloodReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Flags)
                .WithOne(f => f.FloodReport)
                .HasForeignKey(f => f.FloodReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

