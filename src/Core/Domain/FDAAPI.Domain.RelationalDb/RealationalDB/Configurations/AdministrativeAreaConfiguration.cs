using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AdministrativeAreaConfiguration : IEntityTypeConfiguration<AdministrativeArea>
    {
        public void Configure(EntityTypeBuilder<AdministrativeArea> builder)
        {
            builder.ToTable("AdministrativeAreas");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Level).IsRequired().HasMaxLength(20); // "ward", "district", "city"
            builder.Property(e => e.Code).HasMaxLength(50);
            builder.Property(e => e.Geometry).HasColumnType("text"); // JSON string for PostGIS geometry

            // Indexes
            builder.HasIndex(e => e.Level)
                .HasDatabaseName("ix_administrative_areas_level");
            builder.HasIndex(e => e.Code)
                .HasDatabaseName("ix_administrative_areas_code");
            builder.HasIndex(e => e.ParentId)
                .HasDatabaseName("ix_administrative_areas_parent");

            // Self-referencing relationship (hierarchical)
            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

