using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodEventConfiguration : IEntityTypeConfiguration<FloodEvent>
    {
        public void Configure(EntityTypeBuilder<FloodEvent> builder)
        {
            builder.ToTable("FloodEvents");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.AdministrativeAreaId).IsRequired();
            builder.Property(e => e.StartTime).IsRequired();
            builder.Property(e => e.EndTime).IsRequired();
            builder.Property(e => e.PeakLevel).HasColumnType("numeric(14,4)");
            builder.Property(e => e.CreatedAt).IsRequired();

            // Indexes for query performance
            builder.HasIndex(e => new { e.AdministrativeAreaId, e.StartTime })
                .HasDatabaseName("ix_flood_events_area_start");
            builder.HasIndex(e => e.StartTime)
                .HasDatabaseName("ix_flood_events_start_time");

            // Relationship
            builder.HasOne(e => e.AdministrativeArea)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

