using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Constants;

namespace DAL.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "1",
                Name = AppRoles.Admin,
                NormalizedName = AppRoles.Admin.ToUpper(),
                ConcurrencyStamp = "d5a947dc-0bc8-4eb1-998f-410a8801f92e"
            },
            new IdentityRole
            {
                Id = "2",
                Name = AppRoles.ITAdmin,
                NormalizedName = AppRoles.ITAdmin.ToUpper(),
                ConcurrencyStamp = "f623e5a3-7ce8-4f81-9b16-7284fbfec310"
            },
            new IdentityRole
            {
                Id = "3",
                Name = AppRoles.User,
                NormalizedName = AppRoles.User.ToUpper(),
                ConcurrencyStamp = "e18e8071-8bc4-47c1-a20b-ef95db114db7"
            },
            new IdentityRole
            {
                Id = "4",
                Name = AppRoles.TopManager,
                NormalizedName = AppRoles.TopManager.ToUpper(),
                ConcurrencyStamp = "a4249a4f-6d1a-45ea-800d-5872111b22e1"
            }
        );
    }
}
