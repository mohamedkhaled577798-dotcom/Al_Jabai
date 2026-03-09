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
    public DbSet<PropertyRegistrationFile> PropertyRegistrationFiles => Set<PropertyRegistrationFile>();
    public DbSet<MosqueRegistrationFile> MosqueRegistrationFiles => Set<MosqueRegistrationFile>();
    public DbSet<WaqfLandRegistrationFile> WaqfLandRegistrationFiles => Set<WaqfLandRegistrationFile>();
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

    // Phase 3 - Advanced Features
    public DbSet<LegalDispute> LegalDisputes => Set<LegalDispute>();
    public DbSet<DisputeDocument> DisputeDocuments => Set<DisputeDocument>();
    public DbSet<InvestmentContract> InvestmentContracts => Set<InvestmentContract>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<ContractPayment> ContractPayments => Set<ContractPayment>();
    public DbSet<ServiceFacility> ServiceFacilities => Set<ServiceFacility>();
    public DbSet<ServiceImage> ServiceImages => Set<ServiceImage>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<MapIcon> MapIcons => Set<MapIcon>();
    public DbSet<PropertyPricing> PropertyPricings => Set<PropertyPricing>();
    public DbSet<PropertyComparison> PropertyComparisons => Set<PropertyComparison>();
    public DbSet<PropertyComparisonItem> PropertyComparisonItems => Set<PropertyComparisonItem>();
    public DbSet<PropertyServiceAssessment> PropertyServiceAssessments => Set<PropertyServiceAssessment>();
    public DbSet<ServiceProximity> ServiceProximities => Set<ServiceProximity>();

    // Phase 1.4 - سجل التجاوزات
    public DbSet<EncroachmentRecord> EncroachmentRecords => Set<EncroachmentRecord>();
    public DbSet<EncroachmentPhoto> EncroachmentPhotos => Set<EncroachmentPhoto>();

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

        // ========== Phase 3 Configurations ==========
        
        // LegalDispute
        modelBuilder.Entity<LegalDispute>(entity =>
        {
            entity.HasIndex(e => e.CaseNumber).IsUnique();
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DisputeDocument
        modelBuilder.Entity<DisputeDocument>(entity =>
        {
            entity.HasOne(e => e.Dispute)
                  .WithMany(d => d.Documents)
                  .HasForeignKey(e => e.DisputeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // InvestmentContract
        modelBuilder.Entity<InvestmentContract>(entity =>
        {
            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.EndDate);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ContractDocument
        modelBuilder.Entity<ContractDocument>(entity =>
        {
            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Documents)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ContractPayment
        modelBuilder.Entity<ContractPayment>(entity =>
        {
            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Payments)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);
        });

        // ServiceFacility
        modelBuilder.Entity<ServiceFacility>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Location).HasColumnType("geometry");
            entity.Property(e => e.Boundary).HasColumnType("geometry");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ServiceImage
        modelBuilder.Entity<ServiceImage>(entity =>
        {
            entity.HasOne(e => e.ServiceFacility)
                  .WithMany(s => s.Images)
                  .HasForeignKey(e => e.ServiceFacilityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PropertyPricing
        modelBuilder.Entity<PropertyPricing>(entity =>
        {
            entity.HasIndex(e => new { e.ProvinceId, e.DistrictId, e.PropertyTypeId });
            entity.HasIndex(e => e.PriceDate);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PropertyComparison
        modelBuilder.Entity<PropertyComparison>(entity =>
        {
            entity.HasIndex(e => e.ComparisonDate);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PropertyComparisonItem
        modelBuilder.Entity<PropertyComparisonItem>(entity =>
        {
            entity.HasOne(e => e.Comparison)
                  .WithMany(c => c.Items)
                  .HasForeignKey(e => e.ComparisonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PropertyServiceAssessment
        modelBuilder.Entity<PropertyServiceAssessment>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.AssessmentDate);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ServiceProximity
        modelBuilder.Entity<ServiceProximity>(entity =>
        {
            entity.HasOne(e => e.Assessment)
                  .WithMany(a => a.NearbyServices)
                  .HasForeignKey(e => e.AssessmentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ServiceFacility)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceFacilityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // EncroachmentRecord
        modelBuilder.Entity<EncroachmentRecord>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DiscoveryDate);
            entity.Property(e => e.Location).HasColumnType("geometry");
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Province)
                  .WithMany()
                  .HasForeignKey(e => e.ProvinceId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.LegalDispute)
                  .WithMany()
                  .HasForeignKey(e => e.LegalDisputeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // EncroachmentPhoto
        modelBuilder.Entity<EncroachmentPhoto>(entity =>
        {
            entity.HasOne(e => e.Encroachment)
                  .WithMany(r => r.Photos)
                  .HasForeignKey(e => e.EncroachmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceCategory
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasIndex(e => e.NameAr).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ServiceType
        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasIndex(e => new { e.ServiceCategoryId, e.NameAr }).IsUnique();
            entity.HasOne(e => e.ServiceCategory)
                  .WithMany(c => c.ServiceTypes)
                  .HasForeignKey(e => e.ServiceCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.MapIcon)
                  .WithMany()
                  .HasForeignKey(e => e.MapIconId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // MapIcon
        modelBuilder.Entity<MapIcon>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.NameAr });
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
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
