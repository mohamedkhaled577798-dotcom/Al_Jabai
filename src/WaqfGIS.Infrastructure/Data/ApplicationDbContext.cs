using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data;

/// <summary>
/// سياق قاعدة البيانات
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Lookup Tables
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<SubDistrict> SubDistricts => Set<SubDistrict>();
    public DbSet<OfficeType> OfficeTypes => Set<OfficeType>();
    public DbSet<MosqueType> MosqueTypes => Set<MosqueType>();
    public DbSet<MosqueStatus> MosqueStatuses => Set<MosqueStatus>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<UsageType> UsageTypes => Set<UsageType>();

    // Main Tables
    public DbSet<WaqfOffice> WaqfOffices => Set<WaqfOffice>();
    public DbSet<Mosque> Mosques => Set<Mosque>();
    public DbSet<MosqueDocument> MosqueDocuments => Set<MosqueDocument>();
    public DbSet<MosqueImage> MosqueImages => Set<MosqueImage>();
    public DbSet<WaqfProperty> WaqfProperties => Set<WaqfProperty>();
    public DbSet<PropertyDocument> PropertyDocuments => Set<PropertyDocument>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<OfficeImage> OfficeImages => Set<OfficeImage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // GIS Tables - Phase 2
    public DbSet<GisLayer> GisLayers => Set<GisLayer>();
    public DbSet<MosqueBoundary> MosqueBoundaries => Set<MosqueBoundary>();
    public DbSet<PropertyBoundary> PropertyBoundaries => Set<PropertyBoundary>();
    public DbSet<WaqfLand> WaqfLands => Set<WaqfLand>();
    public DbSet<Road> Roads => Set<Road>();
    public DbSet<NearbyProject> NearbyProjects => Set<NearbyProject>();
    public DbSet<GeometryAuditLog> GeometryAuditLogs => Set<GeometryAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ========== GIS Configurations ==========
        
        // MosqueBoundary
        modelBuilder.Entity<MosqueBoundary>(entity =>
        {
            entity.HasIndex(e => e.MosqueId);
            entity.Property(e => e.Boundary).HasColumnType("geometry");
            entity.HasOne(e => e.Mosque)
                  .WithMany(m => m.Boundaries)
                  .HasForeignKey(e => e.MosqueId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WaqfLand
        modelBuilder.Entity<WaqfLand>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.CenterPoint).HasColumnType("geometry");
            entity.Property(e => e.Boundary).HasColumnType("geometry");
            entity.HasOne(e => e.Province)
                  .WithMany()
                  .HasForeignKey(e => e.ProvinceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Road
        modelBuilder.Entity<Road>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Geometry).HasColumnType("geometry");
        });

        // NearbyProject
        modelBuilder.Entity<NearbyProject>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Location).HasColumnType("geometry");
            entity.Property(e => e.Boundary).HasColumnType("geometry");
        });

        // GisLayer
        modelBuilder.Entity<GisLayer>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // GeometryAuditLog
        modelBuilder.Entity<GeometryAuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Timestamp);
        });

        // ========== Spatial Indexes ==========
        // Note: Spatial indexes will be created in migration

        // ========== Global Query Filters ==========
        modelBuilder.Entity<Province>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<District>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SubDistrict>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OfficeType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MosqueType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MosqueStatus>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PropertyType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UsageType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WaqfOffice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Mosque>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WaqfProperty>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WaqfLand>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Road>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<NearbyProject>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MosqueBoundary>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PropertyBoundary>().HasQueryFilter(e => !e.IsDeleted);
        
        // PropertyBoundary configuration
        modelBuilder.Entity<PropertyBoundary>()
            .Property(b => b.Boundary).HasColumnType("geometry");
        modelBuilder.Entity<PropertyBoundary>()
            .HasOne(b => b.Property)
            .WithMany()
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
