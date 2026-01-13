using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.HasIndex(e => e.PhoneNumber);
            
            builder.Property(e => e.IsUsed)
                .HasDefaultValue(false);
            
            builder.Property(e => e.AttemptCount)
                .HasDefaultValue(0);
        }
    }
}
