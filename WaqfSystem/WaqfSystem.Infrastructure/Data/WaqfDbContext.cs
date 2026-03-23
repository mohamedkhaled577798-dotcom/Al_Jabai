using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Infrastructure.Data
{
    public class WaqfDbContext : DbContext, IAppDbContext
    {
        public WaqfDbContext(DbContextOptions<WaqfDbContext> options) : base(options) { }

        // ========== DbSets ==========
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<PropertyAddress> PropertyAddresses { get; set; } = null!;
        public DbSet<PropertyFloor> PropertyFloors { get; set; } = null!;
        public DbSet<PropertyUnit> PropertyUnits { get; set; } = null!;
        public DbSet<PropertyRoom> PropertyRooms { get; set; } = null!;
        public DbSet<PropertyFacility> PropertyFacilities { get; set; } = null!;
        public DbSet<PropertyMeter> PropertyMeters { get; set; } = null!;
        public DbSet<PropertyPartnership> PropertyPartnerships { get; set; } = null!;
        public DbSet<RentContract> RentContracts { get; set; } = null!;
        public DbSet<RentPaymentSchedule> RentPaymentSchedules { get; set; } = null!;
        public DbSet<PropertyRevenue> PropertyRevenues { get; set; } = null!;
        public DbSet<RevenuePeriodLock> RevenuePeriodLocks { get; set; } = null!;
        public DbSet<CollectionBatch> CollectionBatches { get; set; } = null!;
        public DbSet<CollectionSmartLog> CollectionSmartLogs { get; set; } = null!;
        public DbSet<PartnershipConditionRule> PartnershipConditionRules { get; set; } = null!;
        public DbSet<PartnershipExpenseEntry> PartnershipExpenseEntries { get; set; } = null!;
        public DbSet<PartnerRevenueDistribution> PartnerRevenueDistributions { get; set; } = null!;
        public DbSet<PartnerContactLog> PartnerContactLogs { get; set; } = null!;
        public DbSet<PartnerNotificationSchedule> PartnerNotificationSchedules { get; set; } = null!;
        public DbSet<AgriculturalDetail> AgriculturalDetails { get; set; } = null!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        public DbSet<PropertyDocument> PropertyDocuments { get; set; } = null!;
        public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
        public DbSet<DocumentAuditTrail> DocumentAuditTrail { get; set; } = null!;
        public DbSet<DocumentAlert> DocumentAlerts { get; set; } = null!;
        public DbSet<DocumentResponsible> DocumentResponsibles { get; set; } = null!;
        public DbSet<PropertyPhoto> PropertyPhotos { get; set; } = null!;
        public DbSet<PropertyWorkflowHistory> PropertyWorkflowHistories { get; set; } = null!;
        public DbSet<GisSyncLog> GisSyncLogs { get; set; } = null!;
        public DbSet<InspectionMission> InspectionMissions { get; set; } = null!;
        public DbSet<InspectionTeam> InspectionTeams { get; set; } = null!;
        public DbSet<InspectionTeamMember> InspectionTeamMembers { get; set; } = null!;
        public DbSet<MissionStageHistory> MissionStageHistories { get; set; } = null!;
        public DbSet<MissionPropertyEntry> MissionPropertyEntries { get; set; } = null!;
        public DbSet<MissionChecklistTemplate> MissionChecklistTemplates { get; set; } = null!;
        public DbSet<MissionChecklistResult> MissionChecklistResults { get; set; } = null!;
        public DbSet<Partner> Partners { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Governorate> Governorates { get; set; } = null!;
        public DbSet<District> Districts { get; set; } = null!;
        public DbSet<SubDistrict> SubDistricts { get; set; } = null!;
        public DbSet<Neighborhood> Neighborhoods { get; set; } = null!;
        public DbSet<Street> Streets { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserGeographicScope> UserGeographicScopes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserGeographicScopeConfiguration());
            modelBuilder.ApplyConfiguration(new GovernorateConfiguration());
            modelBuilder.ApplyConfiguration(new DistrictConfiguration());
            modelBuilder.ApplyConfiguration(new SubDistrictConfiguration());
            modelBuilder.ApplyConfiguration(new NeighborhoodConfiguration());
            modelBuilder.ApplyConfiguration(new StreetConfiguration());

            modelBuilder.ApplyConfiguration(new InspectionTeamConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionTeamMemberConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionMissionConfiguration());
            modelBuilder.ApplyConfiguration(new MissionStageHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new MissionPropertyEntryConfiguration());
            modelBuilder.ApplyConfiguration(new MissionChecklistTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new MissionChecklistResultConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentVersionConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentAuditTrailConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentAlertConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentResponsibleConfiguration());

            modelBuilder.Entity<Partner>(entity =>
            {
                entity.ToTable("Partners");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Address).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            });

            // ========== PROPERTY ==========
            modelBuilder.Entity<Property>(entity =>
            {
                entity.ToTable("Properties");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WqfNumber).IsUnique();
                entity.HasIndex(e => e.GovernorateId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.ApprovalStage).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.PropertyStatus).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.PropertyType).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.OwnershipType).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.GisFeatureId).HasFilter("[GisFeatureId] IS NOT NULL");
                entity.HasIndex(e => e.GisSyncStatus).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.DqsScore).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.CreatedById).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.LocalId).HasFilter("[LocalId] IS NOT NULL");
                entity.HasIndex(e => new { e.Latitude, e.Longitude }).HasFilter("[Latitude] IS NOT NULL AND [Longitude] IS NOT NULL");
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.Property(e => e.WqfNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PropertyName).HasMaxLength(300).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.PropertyNameEn).HasMaxLength(300);
                entity.Property(e => e.DeedNumber).HasMaxLength(50);
                entity.Property(e => e.CadastralNumber).HasMaxLength(50);
                entity.Property(e => e.TabuNumber).HasMaxLength(50);
                entity.Property(e => e.WaqfOriginStory).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.FounderName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.EndowmentPurpose).HasMaxLength(500).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.FacadeType).HasMaxLength(100).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.RoofType).HasMaxLength(100).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.OwnershipPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.TotalAreaSqm).HasColumnType("decimal(12,2)");
                entity.Property(e => e.BuiltUpAreaSqm).HasColumnType("decimal(12,2)");
                entity.Property(e => e.LandAreaSqm).HasColumnType("decimal(12,2)");
                entity.Property(e => e.EstimatedValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AnnualRevenue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AnnualExpenses).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InsuranceValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DqsScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.GpsAccuracyMeters).HasColumnType("decimal(6,2)");
                entity.Property(e => e.GisFeatureId).HasMaxLength(100);
                entity.Property(e => e.GisPolygonId).HasMaxLength(100);
                entity.Property(e => e.GisLayerName).HasMaxLength(100);
                entity.Property(e => e.SatelliteImageUrl).HasMaxLength(500);
                entity.Property(e => e.LocalId).HasMaxLength(100);
                entity.Property(e => e.DeviceId).HasMaxLength(200);

                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Governorate).WithMany().HasForeignKey(e => e.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY ADDRESS ==========
            modelBuilder.Entity<PropertyAddress>(entity =>
            {
                entity.ToTable("PropertyAddresses");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.StreetId).HasFilter("[StreetId] IS NOT NULL AND [IsDeleted] = 0");

                entity.Property(e => e.BuildingNumber).HasMaxLength(20);
                entity.Property(e => e.PlotNumber).HasMaxLength(20);
                entity.Property(e => e.BlockNumber).HasMaxLength(20);
                entity.Property(e => e.ZoneNumber).HasMaxLength(20);
                entity.Property(e => e.NearestLandmark).HasMaxLength(300).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.AlternativeAddress).HasMaxLength(500).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.What3Words).HasMaxLength(100);
                entity.Property(e => e.PlusCodes).HasMaxLength(100);

                entity.HasOne(e => e.Property).WithOne(p => p.Address).HasForeignKey<PropertyAddress>(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Street).WithMany().HasForeignKey(e => e.StreetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY FLOOR ==========
            modelBuilder.Entity<PropertyFloor>(entity =>
            {
                entity.ToTable("PropertyFloors");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.FloorLabel).HasMaxLength(50).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.TotalAreaSqm).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UsableAreaSqm).HasColumnType("decimal(10,2)");
                entity.Property(e => e.FloorPlanUrl).HasMaxLength(500);

                entity.HasOne(e => e.Property).WithMany(p => p.Floors).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY UNIT ==========
            modelBuilder.Entity<PropertyUnit>(entity =>
            {
                entity.ToTable("PropertyUnits");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.FloorId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.OccupancyStatus).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.UnitNumber).HasMaxLength(20).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.AreaSqm).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MarketRentMonthly).HasColumnType("decimal(12,2)");
                entity.Property(e => e.ElectricMeterNo).HasMaxLength(50);
                entity.Property(e => e.WaterMeterNo).HasMaxLength(50);
                entity.Property(e => e.UnitFloorPlanUrl).HasMaxLength(500);
                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Floor).WithMany(f => f.Units).HasForeignKey(e => e.FloorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Property).WithMany(p => p.Units).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY ROOM ==========
            modelBuilder.Entity<PropertyRoom>(entity =>
            {
                entity.ToTable("PropertyRooms");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.UnitId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.AreaSqm).HasColumnType("decimal(8,2)");
                entity.Property(e => e.Length).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Width).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Notes).HasMaxLength(500).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Unit).WithMany(u => u.Rooms).HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY FACILITY ==========
            modelBuilder.Entity<PropertyFacility>(entity =>
            {
                entity.ToTable("PropertyFacilities");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.Details).HasMaxLength(500).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Property).WithMany(p => p.Facilities).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY METER ==========
            modelBuilder.Entity<PropertyMeter>(entity =>
            {
                entity.ToTable("PropertyMeters");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.UnitId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.MeterNumber).HasMaxLength(50);
                entity.Property(e => e.SubscriberNumber).HasMaxLength(50);
                entity.Property(e => e.IssuingAuthority).HasMaxLength(200).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Unit).WithMany(u => u.Meters).HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY PARTNERSHIP ==========
            modelBuilder.Entity<PropertyPartnership>(entity =>
            {
                entity.ToTable("PropertyPartnerships");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.PartnerName).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.PartnerNationalId).HasMaxLength(20);
                entity.Property(e => e.PartnerSharePercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.PartnerBankIBAN).HasMaxLength(34);
                entity.Property(e => e.AgreementDocUrl).HasMaxLength(500);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.ContactEmail).HasMaxLength(256);
                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Property).WithMany(p => p.Partnerships).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== AGRICULTURAL DETAIL ==========
            modelBuilder.Entity<AgriculturalDetail>(entity =>
            {
                entity.ToTable("AgriculturalDetails");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.TotalAreaDunum).HasColumnType("decimal(12,2)");
                entity.Property(e => e.CultivatedAreaDunum).HasColumnType("decimal(12,2)");
                entity.Property(e => e.UncultivatedAreaDunum).HasColumnType("decimal(12,2)");
                entity.Property(e => e.AverageYieldTonPerDunum).HasColumnType("decimal(8,2)");
                entity.Property(e => e.WaqfShareOfHarvest).HasColumnType("decimal(5,2)");
                entity.Property(e => e.AnnualRevenueEstimate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LandValuePerDunum).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PrimaryHarvestType).HasMaxLength(100).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.SecondaryHarvestType).HasMaxLength(100).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.FarmerName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.FarmerNationalId).HasMaxLength(20);
                entity.Property(e => e.WaterRightsDocUrl).HasMaxLength(500);

                entity.HasOne(e => e.Property).WithOne(p => p.AgriculturalDetail).HasForeignKey<AgriculturalDetail>(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== PROPERTY PHOTO ==========
            modelBuilder.Entity<PropertyPhoto>(entity =>
            {
                entity.ToTable("PropertyPhotos");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.UnitId).HasFilter("[UnitId] IS NOT NULL AND [IsDeleted] = 0");

                entity.Property(e => e.FileUrl).HasMaxLength(500).IsRequired();
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.DeviceAccuracy).HasColumnType("decimal(6,2)");
                entity.Property(e => e.Caption).HasMaxLength(300).UseCollation("Arabic_CI_AS");

                entity.HasOne(e => e.Property).WithMany(p => p.Photos).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Unit).WithMany(u => u.Photos).HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UploadedBy).WithMany().HasForeignKey(e => e.UploadedById).OnDelete(DeleteBehavior.Restrict);
                // CreatedBy is inherited from BaseEntity but PropertyPhoto uses UploadedById instead
                entity.Ignore(e => e.CreatedBy);
            });

            // ========== PROPERTY WORKFLOW HISTORY ==========
            modelBuilder.Entity<PropertyWorkflowHistory>(entity =>
            {
                entity.ToTable("PropertyWorkflowHistory");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.DqsAtAction).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Property).WithMany(p => p.WorkflowHistory).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ActionBy).WithMany().HasForeignKey(e => e.ActionById).OnDelete(DeleteBehavior.Restrict);
                entity.Ignore(e => e.CreatedBy);
            });

            // ========== GIS SYNC LOG ==========
            modelBuilder.Entity<GisSyncLog>(entity =>
            {
                entity.ToTable("GisSyncLogs");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Property).WithMany(p => p.GisSyncLogs).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(e => e.CreatedBy);
            });

            // ========== AUDIT LOG ==========
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TableName, e.RecordId });
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(20).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== NOTIFICATION ==========
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => new { e.UserId, e.IsRead }).HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.Title).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.Message).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.ReferenceTable).HasMaxLength(100);

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(e => e.CreatedBy);
            });

            // ========== USER ==========
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.RoleId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.GovernorateId).HasFilter("[GovernorateId] IS NOT NULL AND [IsDeleted] = 0");

                entity.Property(e => e.FullNameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.FullNameEn).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.NationalId).HasMaxLength(20);
                entity.Property(e => e.RefreshToken).HasMaxLength(500);
                entity.Property(e => e.DeviceId).HasMaxLength(200);
                entity.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);

                entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Governorate).WithMany().HasForeignKey(e => e.GovernorateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== ROLE ==========
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            });

            // ========== GEOGRAPHIC HIERARCHY ==========
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Countries");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
            });

            modelBuilder.Entity<Governorate>(entity =>
            {
                entity.ToTable("Governorates");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.CountryId).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
                entity.HasOne(e => e.Country).WithMany(c => c.Governorates).HasForeignKey(e => e.CountryId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.ToTable("Districts");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.GovernorateId).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
                entity.HasOne(e => e.Governorate).WithMany(g => g.Districts).HasForeignKey(e => e.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SubDistrict>(entity =>
            {
                entity.ToTable("SubDistricts");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.DistrictId).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
                entity.HasOne(e => e.District).WithMany(d => d.SubDistricts).HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Neighborhood>(entity =>
            {
                entity.ToTable("Neighborhoods");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.SubDistrictId).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
                entity.HasOne(e => e.SubDistrict).WithMany(sd => sd.Neighborhoods).HasForeignKey(e => e.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Street>(entity =>
            {
                entity.ToTable("Streets");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.NeighborhoodId).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired().UseCollation("Arabic_CI_AS");
                entity.Property(e => e.NameEn).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
                entity.Property(e => e.GisLayerId).HasMaxLength(100);
                entity.HasOne(e => e.Neighborhood).WithMany(n => n.Streets).HasForeignKey(e => e.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
            });

            // ========== REVENUE ==========
            modelBuilder.Entity<RentContract>(entity =>
            {
                entity.ToTable("RentContracts");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => e.PropertyId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.UnitId).HasFilter("[IsDeleted] = 0");
                entity.HasIndex(e => e.Status).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.TenantNameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS");
                entity.Property(e => e.TenantPhone).HasMaxLength(30);
                entity.Property(e => e.RentAmount).HasColumnType("decimal(15,2)");
                entity.Property(e => e.PenaltyPerDay).HasColumnType("decimal(15,2)");
                entity.Property(e => e.Notes).UseCollation("Arabic_CI_AS");
                entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Floor).WithMany().HasForeignKey(e => e.FloorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Unit).WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RentPaymentSchedule>(entity =>
            {
                entity.ToTable("RentPaymentSchedules");
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
                entity.HasIndex(e => new { e.ContractId, e.PeriodLabel }).HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.PeriodLabel).HasMaxLength(50);
                entity.Property(e => e.ExpectedAmount).HasColumnType("decimal(15,2)");
                entity.HasOne(e => e.Contract).WithMany(c => c.PaymentSchedules).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.ApplyConfiguration(new RentContractConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyRevenueConfiguration());
            modelBuilder.ApplyConfiguration(new RevenuePeriodLockConfiguration());
            modelBuilder.ApplyConfiguration(new RentPaymentScheduleConfiguration());
            modelBuilder.ApplyConfiguration(new CollectionBatchConfiguration());
            modelBuilder.ApplyConfiguration(new CollectionSmartLogConfiguration());

            modelBuilder.ApplyConfiguration(new PartnershipConfiguration());
            modelBuilder.ApplyConfiguration(new PartnershipConditionRuleConfiguration());
            modelBuilder.ApplyConfiguration(new PartnershipExpenseEntryConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerRevenueDistributionConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerContactLogConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerNotificationScheduleConfiguration());

            // ========== SEED DATA ==========
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Country
            modelBuilder.Entity<Country>().HasData(
                new Country { Id = 1, NameAr = "العراق", NameEn = "Iraq", Code = "IQ" }
            );

            // Seed 18 Iraqi Governorates
            modelBuilder.Entity<Governorate>().HasData(
                new Governorate { Id = 1,  CountryId = 1, NameAr = "بغداد",       NameEn = "Baghdad",      Code = "BGW" },
                new Governorate { Id = 2,  CountryId = 1, NameAr = "البصرة",      NameEn = "Basra",        Code = "BSA" },
                new Governorate { Id = 3,  CountryId = 1, NameAr = "نينوى",       NameEn = "Nineveh",      Code = "NIN" },
                new Governorate { Id = 4,  CountryId = 1, NameAr = "أربيل",       NameEn = "Erbil",        Code = "EBL" },
                new Governorate { Id = 5,  CountryId = 1, NameAr = "السليمانية",  NameEn = "Sulaymaniyah", Code = "SLM" },
                new Governorate { Id = 6,  CountryId = 1, NameAr = "دهوك",        NameEn = "Duhok",        Code = "DHK" },
                new Governorate { Id = 7,  CountryId = 1, NameAr = "كركوك",       NameEn = "Kirkuk",       Code = "KRK" },
                new Governorate { Id = 8,  CountryId = 1, NameAr = "ديالى",       NameEn = "Diyala",       Code = "DIY" },
                new Governorate { Id = 9,  CountryId = 1, NameAr = "الأنبار",     NameEn = "Anbar",        Code = "ANB" },
                new Governorate { Id = 10, CountryId = 1, NameAr = "بابل",        NameEn = "Babylon",      Code = "BAB" },
                new Governorate { Id = 11, CountryId = 1, NameAr = "كربلاء",      NameEn = "Karbala",      Code = "KAR" },
                new Governorate { Id = 12, CountryId = 1, NameAr = "النجف",       NameEn = "Najaf",        Code = "NAJ" },
                new Governorate { Id = 13, CountryId = 1, NameAr = "واسط",        NameEn = "Wasit",        Code = "WAS" },
                new Governorate { Id = 14, CountryId = 1, NameAr = "صلاح الدين",  NameEn = "Salahuddin",   Code = "SLD" },
                new Governorate { Id = 15, CountryId = 1, NameAr = "ذي قار",      NameEn = "Dhi Qar",      Code = "DHQ" },
                new Governorate { Id = 16, CountryId = 1, NameAr = "ميسان",       NameEn = "Maysan",       Code = "MYS" },
                new Governorate { Id = 17, CountryId = 1, NameAr = "المثنى",      NameEn = "Muthanna",     Code = "MTN" },
                new Governorate { Id = 18, CountryId = 1, NameAr = "القادسية",    NameEn = "Qadisiyah",    Code = "QAD" }
            );

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1,  NameAr = "مدير النظام",     NameEn = "System Administrator", Code = "SYS_ADMIN" },
                new Role { Id = 2,  NameAr = "مدير الهيئة",     NameEn = "Authority Director",   Code = "AUTH_DIRECTOR" },
                new Role { Id = 3,  NameAr = "مدير إقليمي",     NameEn = "Regional Manager",     Code = "REGIONAL_MGR" },
                new Role { Id = 4,  NameAr = "مراجع قانوني",    NameEn = "Legal Reviewer",       Code = "LEGAL_REVIEWER" },
                new Role { Id = 5,  NameAr = "مهندس",           NameEn = "Engineer",             Code = "ENGINEER" },
                new Role { Id = 6,  NameAr = "مشرف ميداني",     NameEn = "Field Supervisor",     Code = "FIELD_SUPERVISOR" },
                new Role { Id = 7,  NameAr = "باحث ميداني",     NameEn = "Field Inspector",      Code = "FIELD_INSPECTOR" },
                new Role { Id = 8,  NameAr = "جابي",            NameEn = "Collector",            Code = "COLLECTOR" },
                new Role { Id = 9,  NameAr = "مدير العقود",     NameEn = "Contracts Manager",    Code = "CONTRACTS_MGR" },
                new Role { Id = 10, NameAr = "محلل",            NameEn = "Analyst",              Code = "ANALYST" }
            );
            // Seed Admin User (Required for logical consistency since AccountController uses Id=1)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullNameAr = "مدير النظام",
                    Email = "admin@waqf.gov.iq",
                    PasswordHash = "Admin123", // Matches stub login
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    GovernorateId = 1
                }
            );
        }
    }
}
