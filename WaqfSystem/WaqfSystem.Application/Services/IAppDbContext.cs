using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IAppDbContext
    {
        DbSet<Property> Properties { get; }
        DbSet<PropertyAddress> PropertyAddresses { get; }
        DbSet<PropertyFloor> PropertyFloors { get; }
        DbSet<PropertyUnit> PropertyUnits { get; }
        DbSet<PropertyRoom> PropertyRooms { get; }
        DbSet<PropertyFacility> PropertyFacilities { get; }
        DbSet<PropertyMeter> PropertyMeters { get; }
        DbSet<PropertyPartnership> PropertyPartnerships { get; }
        DbSet<RentContract> RentContracts { get; }
        DbSet<RentPaymentSchedule> RentPaymentSchedules { get; }
        DbSet<PropertyRevenue> PropertyRevenues { get; }
        DbSet<RevenuePeriodLock> RevenuePeriodLocks { get; }
        DbSet<CollectionBatch> CollectionBatches { get; }
        DbSet<CollectionSmartLog> CollectionSmartLogs { get; }
        DbSet<AgriculturalDetail> AgriculturalDetails { get; }
        DbSet<DocumentType> DocumentTypes { get; }
        DbSet<PropertyDocument> PropertyDocuments { get; }
        DbSet<DocumentVersion> DocumentVersions { get; }
        DbSet<DocumentAuditTrail> DocumentAuditTrail { get; }
        DbSet<DocumentAlert> DocumentAlerts { get; }
        DbSet<DocumentResponsible> DocumentResponsibles { get; }
        DbSet<PropertyPhoto> PropertyPhotos { get; }
        DbSet<PropertyWorkflowHistory> PropertyWorkflowHistories { get; }
        DbSet<GisSyncLog> GisSyncLogs { get; }
        DbSet<InspectionMission> InspectionMissions { get; }
        DbSet<InspectionTeam> InspectionTeams { get; }
        DbSet<InspectionTeamMember> InspectionTeamMembers { get; }
        DbSet<MissionStageHistory> MissionStageHistories { get; }
        DbSet<MissionPropertyEntry> MissionPropertyEntries { get; }
        DbSet<MissionChecklistTemplate> MissionChecklistTemplates { get; }
        DbSet<MissionChecklistResult> MissionChecklistResults { get; }
        DbSet<Partner> Partners { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<Country> Countries { get; }
        DbSet<Governorate> Governorates { get; }
        DbSet<District> Districts { get; }
        DbSet<SubDistrict> SubDistricts { get; }
        DbSet<Neighborhood> Neighborhoods { get; }
        DbSet<Street> Streets { get; }
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<Permission> Permissions { get; }
        DbSet<RolePermission> RolePermissions { get; }
        DbSet<UserGeographicScope> UserGeographicScopes { get; }

        Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
