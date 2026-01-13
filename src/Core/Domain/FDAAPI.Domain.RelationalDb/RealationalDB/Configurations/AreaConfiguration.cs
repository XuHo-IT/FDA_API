using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Latitude).HasColumnType("numeric(10,6)").IsRequired();
            builder.Property(e => e.Longitude).HasColumnType("numeric(10,6)").IsRequired();
            builder.Property(e => e.RadiusMeters).HasDefaultValue(1000);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired();

            builder.HasIndex(e => e.UserId).HasDatabaseName("ix_areas_user");
            builder.HasIndex(e => new { e.Latitude, e.Longitude }).HasDatabaseName("ix_areas_geo");

            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

