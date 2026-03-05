using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodReportFlagConfiguration : IEntityTypeConfiguration<FloodReportFlag>
    {
        public void Configure(EntityTypeBuilder<FloodReportFlag> builder)
        {
            builder.ToTable("FloodReportFlags");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.FloodReportId)
                .HasDatabaseName("ix_flood_report_flags_report");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_flood_report_flags_user");

            // Unique constraint: one flag per user per report
            builder.HasIndex(x => new { x.FloodReportId, x.UserId })
                .IsUnique()
                .HasDatabaseName("uq_flag");

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("spam | fake | inappropriate");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasCheckConstraint("chk_flag_reason", "reason IN ('spam', 'fake', 'inappropriate')");
        }
    }
}

