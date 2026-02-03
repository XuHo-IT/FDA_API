using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class PredictionLogConfiguration : IEntityTypeConfiguration<PredictionLog>
    {
        public void Configure(EntityTypeBuilder<PredictionLog> builder)
        {
            builder.ToTable("prediction_logs");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            // Properties
            builder.Property(e => e.AreaId).IsRequired(false);  // Nullable: can be for AdministrativeArea prediction
            builder.Property(e => e.AdministrativeAreaId).IsRequired(false);  // Nullable: can be for Area prediction
            builder.Property(e => e.PredictedProb)
                .HasPrecision(5, 4)
                .IsRequired();
            builder.Property(e => e.AiProb)
                .HasPrecision(5, 4);
            builder.Property(e => e.PhysicsProb)
                .HasPrecision(5, 4);
            builder.Property(e => e.RiskLevel)
                .HasMaxLength(20);
            builder.Property(e => e.StartTime)
                .IsRequired();
            builder.Property(e => e.EndTime)
                .IsRequired();
            builder.Property(e => e.ActualWaterLevel)
                .HasPrecision(14, 4);
            builder.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .IsRequired();
            builder.Property(e => e.IsCorrect);
            builder.Property(e => e.AccuracyScore)
                .HasPrecision(5, 4);
            builder.Property(e => e.VerifiedAt);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired();

            // Constraints
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("chk_prob_range", "predicted_prob >= 0 AND predicted_prob <= 1");
                t.HasCheckConstraint("chk_time_range", "end_time > start_time");
            });

            // Indexes
            builder.HasIndex(e => new { e.AreaId, e.StartTime })
                .HasDatabaseName("ix_prediction_logs_area_time");
            builder.HasIndex(e => new { e.IsVerified, e.EndTime })
                .HasDatabaseName("ix_prediction_logs_verified");
            builder.HasIndex(e => e.EndTime)
                .HasDatabaseName("ix_prediction_logs_end_time")
                .HasFilter("\"IsVerified\" = false");

            // Relationships
            builder.HasOne(e => e.Area)
                .WithMany()
                .HasForeignKey(e => e.AreaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            
            builder.HasOne(e => e.AdministrativeArea)
                .WithMany()
                .HasForeignKey(e => e.AdministrativeAreaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            
            // Constraint: Either AreaId or AdministrativeAreaId must be set (enforced at application level)
        }
    }
}

