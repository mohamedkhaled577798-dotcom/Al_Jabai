using Microsoft.EntityFrameworkCore.Storage;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Infrastructure.Data;

namespace WaqfGIS.Infrastructure.Repositories;

/// <summary>
/// وحدة العمل
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IRepository<Province>? _provinces;
    private IRepository<District>? _districts;
    private IRepository<SubDistrict>? _subDistricts;
    private IRepository<OfficeType>? _officeTypes;
    private IRepository<WaqfOffice>? _waqfOffices;
    private IRepository<MosqueType>? _mosqueTypes;
    private IRepository<MosqueStatus>? _mosqueStatuses;
    private IRepository<Mosque>? _mosques;
    private IRepository<MosqueDocument>? _mosqueDocuments;
    private IRepository<MosqueImage>? _mosqueImages;
    private IRepository<PropertyType>? _propertyTypes;
    private IRepository<UsageType>? _usageTypes;
    private IRepository<WaqfProperty>? _waqfProperties;
    private IRepository<PropertyDocument>? _propertyDocuments;
    private IRepository<PropertyImage>? _propertyImages;
    private IRepository<OfficeImage>? _officeImages;
    private IRepository<AuditLog>? _auditLogs;

    public IRepository<Province> Provinces => _provinces ??= new Repository<Province>(_context);
    public IRepository<District> Districts => _districts ??= new Repository<District>(_context);
    public IRepository<SubDistrict> SubDistricts => _subDistricts ??= new Repository<SubDistrict>(_context);
    public IRepository<OfficeType> OfficeTypes => _officeTypes ??= new Repository<OfficeType>(_context);
    public IRepository<WaqfOffice> WaqfOffices => _waqfOffices ??= new Repository<WaqfOffice>(_context);
    public IRepository<MosqueType> MosqueTypes => _mosqueTypes ??= new Repository<MosqueType>(_context);
    public IRepository<MosqueStatus> MosqueStatuses => _mosqueStatuses ??= new Repository<MosqueStatus>(_context);
    public IRepository<Mosque> Mosques => _mosques ??= new Repository<Mosque>(_context);
    public IRepository<MosqueDocument> MosqueDocuments => _mosqueDocuments ??= new Repository<MosqueDocument>(_context);
    public IRepository<MosqueImage> MosqueImages => _mosqueImages ??= new Repository<MosqueImage>(_context);
    public IRepository<PropertyType> PropertyTypes => _propertyTypes ??= new Repository<PropertyType>(_context);
    public IRepository<UsageType> UsageTypes => _usageTypes ??= new Repository<UsageType>(_context);
    public IRepository<WaqfProperty> WaqfProperties => _waqfProperties ??= new Repository<WaqfProperty>(_context);
    public IRepository<PropertyDocument> PropertyDocuments => _propertyDocuments ??= new Repository<PropertyDocument>(_context);
    public IRepository<PropertyImage> PropertyImages => _propertyImages ??= new Repository<PropertyImage>(_context);
    public IRepository<OfficeImage> OfficeImages => _officeImages ??= new Repository<OfficeImage>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    // Dictionary to cache generic repositories
    private readonly Dictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<T>(_context);
        }
        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
