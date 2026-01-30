using WaqfGIS.Core.Entities;

namespace WaqfGIS.Core.Interfaces;

/// <summary>
/// وحدة العمل
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Province> Provinces { get; }
    IRepository<District> Districts { get; }
    IRepository<SubDistrict> SubDistricts { get; }
    IRepository<OfficeType> OfficeTypes { get; }
    IRepository<WaqfOffice> WaqfOffices { get; }
    IRepository<MosqueType> MosqueTypes { get; }
    IRepository<MosqueStatus> MosqueStatuses { get; }
    IRepository<Mosque> Mosques { get; }
    IRepository<MosqueDocument> MosqueDocuments { get; }
    IRepository<MosqueImage> MosqueImages { get; }
    IRepository<PropertyType> PropertyTypes { get; }
    IRepository<UsageType> UsageTypes { get; }
    IRepository<WaqfProperty> WaqfProperties { get; }
    IRepository<PropertyDocument> PropertyDocuments { get; }
    IRepository<PropertyImage> PropertyImages { get; }
    IRepository<OfficeImage> OfficeImages { get; }
    IRepository<AuditLog> AuditLogs { get; }

    // Generic repository access for GIS entities
    IRepository<T> Repository<T>() where T : BaseEntity;

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
