using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class FloodReportVoteConfiguration : IEntityTypeConfiguration<FloodReportVote>
    {
        public void Configure(EntityTypeBuilder<FloodReportVote> builder)
        {
            builder.ToTable("FloodReportVotes");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.FloodReportId)
                .HasDatabaseName("ix_flood_report_votes_report");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_flood_report_votes_user");

            // Unique constraint: one vote per user per report
            builder.HasIndex(x => new { x.FloodReportId, x.UserId })
                .IsUnique()
                .HasDatabaseName("uq_vote");

            builder.Property(x => x.VoteType)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("up")
                .HasComment("up | down");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasCheckConstraint("chk_vote_type", "vote_type IN ('up', 'down')");
        }
    }
}

