using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data.Configurations;

public class WaqfPropertyConfiguration : IEntityTypeConfiguration<WaqfProperty>
{
    public void Configure(EntityTypeBuilder<WaqfProperty> builder)
    {
        builder.ToTable("WaqfProperties");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Uuid).IsRequired();
        builder.HasIndex(e => e.Uuid).IsUnique();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(e => e.NameEn).HasMaxLength(200);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.AreaSqm).HasColumnType("decimal(12,2)");
        builder.Property(e => e.BuiltAreaSqm).HasColumnType("decimal(12,2)");
        builder.Property(e => e.EstimatedValue).HasColumnType("decimal(15,2)");
        builder.Property(e => e.MonthlyRent).HasColumnType("decimal(12,2)");
        builder.Property(e => e.AnnualRent).HasColumnType("decimal(12,2)");
        builder.Property(e => e.Currency).HasMaxLength(3);
        builder.Property(e => e.TenantName).HasMaxLength(200);
        builder.Property(e => e.Location).HasColumnType("geography").IsRequired();

        // Relationships
        builder.HasOne(e => e.WaqfOffice)
            .WithMany(o => o.WaqfProperties)
            .HasForeignKey(e => e.WaqfOfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PropertyType)
            .WithMany(t => t.WaqfProperties)
            .HasForeignKey(e => e.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UsageType)
            .WithMany(u => u.WaqfProperties)
            .HasForeignKey(e => e.UsageTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Province)
            .WithMany(p => p.WaqfProperties)
            .HasForeignKey(e => e.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
