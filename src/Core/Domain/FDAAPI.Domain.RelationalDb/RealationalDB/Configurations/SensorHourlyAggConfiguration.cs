using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class SensorHourlyAggConfiguration : IEntityTypeConfiguration<SensorHourlyAgg>
    {
        public void Configure(EntityTypeBuilder<SensorHourlyAgg> builder)
        {
            builder.ToTable("sensor_hourly_agg");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.StationId)
                .HasColumnName("station_id")
                .IsRequired();

            builder.Property(e => e.HourStart)
                .HasColumnName("hour_start")
                .IsRequired();

            builder.Property(e => e.MaxLevel)
                .HasColumnName("max_level")
                .HasPrecision(14, 4);

            builder.Property(e => e.MinLevel)
                .HasColumnName("min_level")
                .HasPrecision(14, 4);

            builder.Property(e => e.AvgLevel)
                .HasColumnName("avg_level")
                .HasPrecision(14, 4);

            builder.Property(e => e.ReadingCount)
                .HasColumnName("reading_count");

            builder.Property(e => e.QualityScore)
                .HasColumnName("quality_score")
                .HasPrecision(5, 2);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indexes
            builder.HasIndex(e => new { e.StationId, e.HourStart })
                .IsUnique()
                .HasDatabaseName("uq_hourly_agg_station_hour");

            builder.HasIndex(e => e.HourStart)
                .HasDatabaseName("ix_hourly_agg_hour");

            // Relationships
            builder.HasOne(e => e.Station)
                .WithMany()
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
