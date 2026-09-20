using Microsoft.AspNetCore.Identity;
using DAL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.UserCode)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FullName)
            .HasMaxLength(255);

        builder.HasIndex(u => u.UserCode)
            .IsUnique();

        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity<IdentityUserRole<string>>(
                j => j.HasOne<IdentityRole>().WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId));

        builder.HasData(
            new User
            {
                Id = "1",
                UserName = "Admin",
                Email = "admin@egyptair.com",
                NormalizedUserName = "ADMIN",
                NormalizedEmail = "ADMIN@EGYPTAIR.COM",
                UserCode = "1",
                FullName = "Admin",
                IsActive = true,
                NewUser = false,
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEPFctG6LS8KiYEVdI0doV499jipmFK/3jNgFVjDcPHTor8FuU9BswnPGKzXHt2gLaA==",
                ConcurrencyStamp = "d3f6ea1c-c78d-4fc1-80fd-e7c49d4a59ef",
                SecurityStamp = "c039b90a-61bb-4c70-9f6b-44aa63d486cf"
            },
            new User
            {
                Id = "2",
                UserName = "It Admin",
                Email = "itadmin@egyptair.com",
                NormalizedUserName = "IT ADMIN",
                NormalizedEmail = "ITADMIN@EGYPTAIR.COM",
                UserCode = "2",
                FullName = "It Admin",
                IsActive = true,
                NewUser = false,
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEPFctG6LS8KiYEVdI0doV499jipmFK/3jNgFVjDcPHTor8FuU9BswnPGKzXHt2gLaA==",
                ConcurrencyStamp = "52d7a6e0-4c08-4943-8fa1-b99c47d5ce00",
                SecurityStamp = "e2f4d42b-773f-4d54-abde-7cc86cf70a1a"
            },
            new User
            {
                Id = "3",
                UserName = "User",
                Email = "user@egyptair.com",
                NormalizedUserName = "USER",
                NormalizedEmail = "USER@EGYPTAIR.COM",
                UserCode = "3",
                FullName = "User",
                IsActive = true,
                NewUser = false,
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEPFctG6LS8KiYEVdI0doV499jipmFK/3jNgFVjDcPHTor8FuU9BswnPGKzXHt2gLaA==",
                ConcurrencyStamp = "8f3b23e1-9d1a-42c8-89c0-112233445566",
                SecurityStamp = "99887766-5544-3322-1100-aabbccddeeff"
            },
            new User
            {
                Id = "4",
                UserName = "Top Manager",
                Email = "topmanager@egyptair.com",
                NormalizedUserName = "TOP MANAGER",
                NormalizedEmail = "TOPMANAGER@EGYPTAIR.COM",
                UserCode = "4",
                FullName = "Top Manager",
                IsActive = true,
                NewUser = false,
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEPFctG6LS8KiYEVdI0doV499jipmFK/3jNgFVjDcPHTor8FuU9BswnPGKzXHt2gLaA==",
                ConcurrencyStamp = "77665544-3322-1100-ffeeddccbbaa",
                SecurityStamp = "11223344-5566-7788-99aa-bbccddeeff00"
            }
        );
    }
}
