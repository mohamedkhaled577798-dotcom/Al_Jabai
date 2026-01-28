using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data.Configurations;

public class MosqueConfiguration : IEntityTypeConfiguration<Mosque>
{
    public void Configure(EntityTypeBuilder<Mosque> builder)
    {
        builder.ToTable("Mosques");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Uuid).IsRequired();
        builder.HasIndex(e => e.Uuid).IsUnique();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(e => e.NameEn).HasMaxLength(200);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Neighborhood).HasMaxLength(100);
        builder.Property(e => e.NearestLandmark).HasMaxLength(200);
        builder.Property(e => e.AreaSqm).HasColumnType("decimal(10,2)");
        builder.Property(e => e.ImamName).HasMaxLength(100);
        builder.Property(e => e.ImamPhone).HasMaxLength(20);
        builder.Property(e => e.MuezzinName).HasMaxLength(100);
        builder.Property(e => e.DeedNumber).HasMaxLength(50);
        builder.Property(e => e.RegistrationNumber).HasMaxLength(50);

        // Spatial column
        builder.Property(e => e.Location).HasColumnType("geography").IsRequired();

        // Relationships
        builder.HasOne(e => e.WaqfOffice)
            .WithMany(o => o.Mosques)
            .HasForeignKey(e => e.WaqfOfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MosqueType)
            .WithMany(t => t.Mosques)
            .HasForeignKey(e => e.MosqueTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MosqueStatus)
            .WithMany(s => s.Mosques)
            .HasForeignKey(e => e.MosqueStatusId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Province)
            .WithMany(p => p.Mosques)
            .HasForeignKey(e => e.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.District)
            .WithMany(d => d.Mosques)
            .HasForeignKey(e => e.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
