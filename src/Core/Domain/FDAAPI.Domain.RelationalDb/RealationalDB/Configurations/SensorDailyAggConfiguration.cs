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
    public class SensorDailyAggConfiguration : IEntityTypeConfiguration<SensorDailyAgg>
    {
        public void Configure(EntityTypeBuilder<SensorDailyAgg> builder)
        {
            builder.ToTable("sensor_daily_agg");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.StationId)
                .HasColumnName("station_id")
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
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

            builder.Property(e => e.RainfallTotal)
                .HasColumnName("rainfall_total")
                .HasPrecision(14, 4);

            builder.Property(e => e.ReadingCount)
                .HasColumnName("reading_count");

            builder.Property(e => e.FloodHours)
                .HasColumnName("flood_hours");

            builder.Property(e => e.PeakSeverity)
                .HasColumnName("peak_severity");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indexes
            builder.HasIndex(e => new { e.StationId, e.Date })
                .IsUnique()
                .HasDatabaseName("uq_daily_agg_station_date");

            builder.HasIndex(e => e.Date)
                .HasDatabaseName("ix_daily_agg_date");

            // Relationships
            builder.HasOne(e => e.Station)
                .WithMany()
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
