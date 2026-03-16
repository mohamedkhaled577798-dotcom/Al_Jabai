using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly WaqfDbContext _context;

        public PropertyRepository(WaqfDbContext context)
        {
            _context = context;
        }

        public async Task<Property?> GetByIdAsync(int id)
        {
            return await _context.Properties.FindAsync(id);
        }

        public async Task<Property?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.Address)
                    .ThenInclude(a => a.Street)
                        .ThenInclude(s => s.Neighborhood)
                            .ThenInclude(n => n.SubDistrict)
                                .ThenInclude(sd => sd.District)
                                    .ThenInclude(d => d.Governorate)
                .Include(p => p.Floors).ThenInclude(f => f.Units).ThenInclude(u => u.Rooms)
                .Include(p => p.Floors).ThenInclude(f => f.Units).ThenInclude(u => u.Meters)
                .Include(p => p.Facilities)
                .Include(p => p.Partnerships).ThenInclude(pp => pp.RevenueDistributions)
                .Include(p => p.Documents).ThenInclude(d => d.DocumentType)
                .Include(p => p.Documents).ThenInclude(d => d.CurrentVersion)
                .Include(p => p.Documents).ThenInclude(d => d.VerifiedBy)
                .Include(p => p.Photos)
                .Include(p => p.WorkflowHistory)
                .Include(p => p.AgriculturalDetail)
                .Include(p => p.Governorate)
                .Include(p => p.CreatedBy)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Property?> GetByWqfNumberAsync(string wqfNumber)
        {
            return await _context.Properties.FirstOrDefaultAsync(p => p.WqfNumber == wqfNumber);
        }

        public async Task<Property?> GetByLocalIdAsync(string localId)
        {
            return await _context.Properties.FirstOrDefaultAsync(p => p.LocalId == localId);
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<Property> AddAsync(Property property)
        {
            var entry = await _context.Properties.AddAsync(property);
            return entry.Entity;
        }

        public Task UpdateAsync(Property property)
        {
            property.UpdatedAt = DateTime.UtcNow;
            _context.Properties.Update(property);
            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(int id)
        {
            var property = await GetByIdAsync(id);
            if (property != null)
            {
                property.IsDeleted = true;
                property.UpdatedAt = DateTime.UtcNow;
                _context.Properties.Update(property);
            }
        }

        public IQueryable<Property> GetQueryable()
        {
            return _context.Properties.AsQueryable();
        }

        public async Task<(IEnumerable<Property> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            int? governorateId = null,
            PropertyType? propertyType = null,
            OwnershipType? ownershipType = null,
            ApprovalStage? approvalStage = null,
            PropertyStatus? status = null,
            string? searchTerm = null,
            int? createdById = null)
        {
            var query = _context.Properties
                .Include(p => p.Governorate)
                .Include(p => p.CreatedBy)
                .AsQueryable();

            if (governorateId.HasValue)
                query = query.Where(p => p.GovernorateId == governorateId.Value);

            if (propertyType.HasValue)
                query = query.Where(p => p.PropertyType == propertyType.Value);

            if (ownershipType.HasValue)
                query = query.Where(p => p.OwnershipType == ownershipType.Value);

            if (approvalStage.HasValue)
                query = query.Where(p => p.ApprovalStage == approvalStage.Value);

            if (status.HasValue)
                query = query.Where(p => p.PropertyStatus == status.Value);

            if (createdById.HasValue)
                query = query.Where(p => p.CreatedById == createdById.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    (p.PropertyName != null && p.PropertyName.Contains(searchTerm)) ||
                    p.WqfNumber.Contains(searchTerm) ||
                    (p.DeedNumber != null && p.DeedNumber.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Property>> GetNearbyAsync(decimal latitude, decimal longitude, double radiusMeters, int limit = 20)
        {
            // Simple bounding-box approximation for nearby search
            // 1 degree latitude ≈ 111,320 meters
            var latDelta = (decimal)(radiusMeters / 111320.0);
            var lngDelta = (decimal)(radiusMeters / (111320.0 * Math.Cos((double)latitude * Math.PI / 180.0)));

            return await _context.Properties
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue &&
                            p.Latitude >= latitude - latDelta && p.Latitude <= latitude + latDelta &&
                            p.Longitude >= longitude - lngDelta && p.Longitude <= longitude + lngDelta)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetByGisSyncStatusAsync(GisSyncStatus status)
        {
            return await _context.Properties
                .Where(p => p.GisSyncStatus == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetWithPendingGisSyncAsync()
        {
            return await GetByGisSyncStatusAsync(GisSyncStatus.Pending);
        }

        public async Task<int> GetCountByGovernorateAsync(int governorateId)
        {
            return await _context.Properties.CountAsync(p => p.GovernorateId == governorateId);
        }

        public async Task<int> GetCountByApprovalStageAsync(ApprovalStage stage)
        {
            return await _context.Properties.CountAsync(p => p.ApprovalStage == stage);
        }

        public async Task<Dictionary<int, int>> GetCountsByGovernorateAsync()
        {
            return await _context.Properties
                .Where(p => p.GovernorateId.HasValue)
                .GroupBy(p => p.GovernorateId!.Value)
                .Select(g => new { Id = g.Key, Valdez = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Valdez);
        }

        public async Task<decimal> GetAverageDqsScoreAsync(int? governorateId = null)
        {
            var query = _context.Properties.AsQueryable();
            if (governorateId.HasValue)
                query = query.Where(p => p.GovernorateId == governorateId.Value);

            var avg = await query.AverageAsync(p => (decimal?)p.DqsScore);
            return avg ?? 0;
        }

        public async Task<IEnumerable<PropertyFloor>> GetFloorsByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyFloors
                .Where(f => f.PropertyId == propertyId)
                .OrderBy(f => f.FloorNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyUnit>> GetUnitsByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyUnits
                .Where(u => u.PropertyId == propertyId)
                .Include(u => u.Floor)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyUnit>> GetUnitsByFloorIdAsync(int floorId)
        {
            return await _context.PropertyUnits
                .Where(u => u.FloorId == floorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyDocument>> GetDocumentsByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyDocuments
                .Where(d => d.PropertyId == propertyId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyPhoto>> GetPhotosByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyPhotos
                .Where(p => p.PropertyId == propertyId)
                .OrderByDescending(p => p.IsMain)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyPartnership>> GetPartnershipsByPropertyIdAsync(int propertyId)
        {
            return await _context.PropertyPartnerships
                .Where(p => p.PropertyId == propertyId && p.IsActive)
                .ToListAsync();
        }

        public async Task<AgriculturalDetail?> GetAgriculturalDetailByPropertyIdAsync(int propertyId)
        {
            return await _context.AgriculturalDetails
                .FirstOrDefaultAsync(a => a.PropertyId == propertyId);
        }

        public async Task<IEnumerable<PropertyWorkflowHistory>> GetWorkflowHistoryAsync(int propertyId)
        {
            return await _context.PropertyWorkflowHistories
                .Where(w => w.PropertyId == propertyId)
                .Include(w => w.ActionBy)
                .OrderByDescending(w => w.ActionAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetMapPointsAsync(
            int? governorateId = null,
            PropertyType? propertyType = null,
            ApprovalStage? approvalStage = null)
        {
            var query = _context.Properties
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .AsQueryable();

            if (governorateId.HasValue)
                query = query.Where(p => p.GovernorateId == governorateId.Value);
            if (propertyType.HasValue)
                query = query.Where(p => p.PropertyType == propertyType.Value);
            if (approvalStage.HasValue)
                query = query.Where(p => p.ApprovalStage == approvalStage.Value);

            return await query
                .Select(p => new Property
                {
                    Id = p.Id,
                    WqfNumber = p.WqfNumber,
                    PropertyName = p.PropertyName,
                    PropertyType = p.PropertyType,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ApprovalStage = p.ApprovalStage,
                    DqsScore = p.DqsScore,
                    GovernorateId = p.GovernorateId
                })
                .ToListAsync();
        }
    }
}
