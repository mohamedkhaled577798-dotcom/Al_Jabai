using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NameEn).HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(15);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.AreaSqKm).HasColumnType("decimal(12,2)");
        builder.Property(e => e.Boundary).HasColumnType("geography");
        builder.Property(e => e.Centroid).HasColumnType("geography");

        builder.HasOne(e => e.Province)
            .WithMany(p => p.Districts)
            .HasForeignKey(e => e.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
