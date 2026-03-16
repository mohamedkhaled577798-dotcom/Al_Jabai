using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Infrastructure.Data
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.NameAr).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.NameEn).HasMaxLength(100);
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DisplayNameAr).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DisplayNameEn).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Color).HasMaxLength(20);
            builder.Property(x => x.Icon).HasMaxLength(50);
            builder.Property(x => x.GeographicScopeLevel).HasConversion<byte>().HasDefaultValue(GeographicScopeLevel.None);
            builder.Property(x => x.HasGlobalScope).HasDefaultValue(false);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasMany(x => x.Permissions).WithOne(x => x.Role).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Users).WithOne(x => x.Role).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PermissionKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Module).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DisplayNameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.DisplayNameEn).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => x.PermissionKey).IsUnique();
            builder.HasIndex(x => new { x.Module, x.IsActive });
        }
    }

    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(x => new { x.RoleId, x.PermissionId });

            builder.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.GrantedBy).WithMany(x => x.GrantedRolePermissions).HasForeignKey(x => x.GrantedById).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_RolePerms_Role");
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.FullNameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.FullNameEn).HasMaxLength(200);
            builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(20);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20);
            builder.Property(x => x.Phone2).HasMaxLength(20);
            builder.Property(x => x.NationalId).HasMaxLength(20);
            builder.Property(x => x.JobTitle).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.LockReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.LastLoginIp).HasMaxLength(50);
            builder.Property(x => x.ProfilePhotoUrl).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(1000).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_Users_Role");
            builder.HasIndex(x => x.GovernorateId).HasDatabaseName("IX_Users_Gov");
            builder.HasIndex(x => x.DistrictId).HasDatabaseName("IX_Users_District");
            builder.HasIndex(x => x.SubDistrictId).HasDatabaseName("IX_Users_SubDistrict");

            builder.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Governorate).WithMany().HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubDistrict).WithMany().HasForeignKey(x => x.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LockedBy).WithMany().HasForeignKey(x => x.LockedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UserGeographicScopeConfiguration : IEntityTypeConfiguration<UserGeographicScope>
    {
        public void Configure(EntityTypeBuilder<UserGeographicScope> builder)
        {
            builder.ToTable("UserGeographicScopes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ScopeLevel).HasConversion<byte>();
            builder.HasIndex(x => new { x.UserId, x.ScopeLevel, x.GovernorateId, x.DistrictId, x.SubDistrictId }).HasDatabaseName("IX_UserGeoScope_UserScope");

            builder.HasOne(x => x.User).WithMany(x => x.GeographicScopes).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Governorate).WithMany().HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubDistrict).WithMany().HasForeignKey(x => x.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.Property(x => x.NameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
            builder.Property(x => x.GisLayerId).HasMaxLength(100);
            builder.Property(x => x.PostalCode).HasMaxLength(20);
            builder.HasIndex(x => new { x.IsActive, x.SortOrder }).HasDatabaseName("IX_Gov_Active");
        }
    }

    public class DistrictConfiguration : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.Property(x => x.NameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
            builder.Property(x => x.GisLayerId).HasMaxLength(100);
            builder.HasIndex(x => new { x.GovernorateId, x.IsActive }).HasDatabaseName("IX_District_Gov");
        }
    }

    public class SubDistrictConfiguration : IEntityTypeConfiguration<SubDistrict>
    {
        public void Configure(EntityTypeBuilder<SubDistrict> builder)
        {
            builder.Property(x => x.NameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.GisLayerId).HasMaxLength(100);
            builder.HasIndex(x => new { x.DistrictId, x.IsActive }).HasDatabaseName("IX_SubDist_Dist");
        }
    }

    public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
    {
        public void Configure(EntityTypeBuilder<Neighborhood> builder)
        {
            builder.Property(x => x.NameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Type).HasMaxLength(20).HasDefaultValue("City");
            builder.Property(x => x.PostalCode).HasMaxLength(20);
            builder.Property(x => x.GisLayerId).HasMaxLength(100);
            builder.HasIndex(x => new { x.SubDistrictId, x.IsActive }).HasDatabaseName("IX_Neigh_Sub");
        }
    }

    public class StreetConfiguration : IEntityTypeConfiguration<Street>
    {
        public void Configure(EntityTypeBuilder<Street> builder)
        {
            builder.Property(x => x.NameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.GisLayerId).HasMaxLength(100);
            builder.Property(x => x.GisLineId).HasMaxLength(100);
            builder.Property(x => x.StreetType).HasMaxLength(30);
            builder.HasIndex(x => new { x.NeighborhoodId, x.IsActive }).HasDatabaseName("IX_Street_Neigh");
        }
    }
}
