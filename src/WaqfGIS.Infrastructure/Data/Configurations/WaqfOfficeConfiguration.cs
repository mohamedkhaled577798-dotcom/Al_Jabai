using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data.Configurations;

public class WaqfOfficeConfiguration : IEntityTypeConfiguration<WaqfOffice>
{
    public void Configure(EntityTypeBuilder<WaqfOffice> builder)
    {
        builder.ToTable("WaqfOffices");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Uuid).IsRequired();
        builder.HasIndex(e => e.Uuid).IsUnique();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(e => e.NameEn).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.Location).HasColumnType("geography");

        // Self-referencing relationship
        builder.HasOne(e => e.ParentOffice)
            .WithMany(o => o.ChildOffices)
            .HasForeignKey(e => e.ParentOfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OfficeType)
            .WithMany(t => t.WaqfOffices)
            .HasForeignKey(e => e.OfficeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
