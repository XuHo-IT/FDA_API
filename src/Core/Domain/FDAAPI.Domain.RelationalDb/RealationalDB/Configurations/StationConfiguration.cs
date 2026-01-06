using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {

            builder.ToTable("stations");

            builder.HasKey(x => x.Id);

            //  Index, Unique
            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasDatabaseName("ux_station_code");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("ix_station_status");

            builder.HasIndex(x => new { x.Latitude, x.Longitude })
                .HasDatabaseName("ix_station_geo");

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .HasMaxLength(255);

            builder.Property(x => x.LocationDesc)
                .HasColumnType("text");

            // Precision 
            builder.Property(x => x.Latitude)
                .HasPrecision(10, 6);

            builder.Property(x => x.Longitude)
                .HasPrecision(10, 6);

            builder.Property(x => x.RoadName)
                .HasMaxLength(255);

            builder.Property(x => x.Direction)
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasMaxLength(20);

            // Time
            builder.Property(x => x.CreatedAt)
                .IsRequired();
        }
    }
}
