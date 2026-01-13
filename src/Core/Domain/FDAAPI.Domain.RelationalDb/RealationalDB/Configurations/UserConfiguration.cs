using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.HasIndex(e => e.Email)
                .IsUnique();
            
            builder.HasIndex(e => e.PhoneNumber)
                .IsUnique();
            
            builder.Property(e => e.Status)
                .HasDefaultValue("ACTIVE");
            
            builder.Property(e => e.Provider)
                .HasDefaultValue("local");
        }
    }
}
