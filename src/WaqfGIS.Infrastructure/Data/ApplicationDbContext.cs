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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
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
