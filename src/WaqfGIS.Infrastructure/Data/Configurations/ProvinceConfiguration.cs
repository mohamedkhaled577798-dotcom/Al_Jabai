using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NameEn).HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(10);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.AreaSqKm).HasColumnType("decimal(12,2)");

        // Spatial column - SQL Server geography
        builder.Property(e => e.Boundary).HasColumnType("geography");
        builder.Property(e => e.Centroid).HasColumnType("geography");
    }
}
