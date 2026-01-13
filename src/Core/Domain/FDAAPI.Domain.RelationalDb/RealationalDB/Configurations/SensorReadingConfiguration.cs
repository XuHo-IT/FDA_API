using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            
            builder.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsRequired();
            
            builder.Property(e => e.Value)
                .IsRequired();
            
            builder.Property(e => e.MeasuredAt)
                .IsRequired();
            
            builder.Property(e => e.CreatedAt)
                .IsRequired();
            
            builder.Property(e => e.UpdatedAt)
                .IsRequired();
            
            builder.HasIndex(e => e.StationId)
                .HasDatabaseName("ix_sensor_readings_station");
            
            builder.HasIndex(e => e.MeasuredAt)
                .HasDatabaseName("ix_sensor_readings_measured_at");
            
            builder.HasIndex(e => new { e.StationId, e.MeasuredAt })
                .HasDatabaseName("ix_sensor_readings_station_time");
            
            builder.HasOne(e => e.Station)
                .WithMany()
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
