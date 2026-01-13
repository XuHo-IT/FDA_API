using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FDAAPI.Domain.RelationalDb.RealationalDB.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.HasIndex(e => e.Code)
                .IsUnique();
            
            // Seed initial Roles
            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Code = "ADMIN",
                    Name = "Administrator"
                },
                new Role
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Code = "AUTHORITY",
                    Name = "Authority Officer"
                },
                new Role
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Code = "USER",
                    Name = "Citizen User"
                },
                new Role
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Code = "SUPERADMIN",
                    Name = "Super Administrator"
                }
            );
        }
    }
}
