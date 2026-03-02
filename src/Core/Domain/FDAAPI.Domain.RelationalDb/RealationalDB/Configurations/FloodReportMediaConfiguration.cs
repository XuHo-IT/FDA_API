using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodReportMediaConfiguration : IEntityTypeConfiguration<FloodReportMedia>
    {
        public void Configure(EntityTypeBuilder<FloodReportMedia> builder)
        {
            builder.ToTable("FloodReportMedia");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.FloodReportId)
                .HasDatabaseName("ix_flood_report_media_report");

            builder.Property(x => x.MediaType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("photo")
                .HasComment("photo | video");

            builder.Property(x => x.MediaUrl)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.ThumbnailUrl)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasCheckConstraint("chk_media_type", "media_type IN ('photo', 'video')");
        }
    }
}

