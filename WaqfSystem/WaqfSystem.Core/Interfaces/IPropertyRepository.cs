using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Interfaces
{
    /// <summary>
    /// Repository interface for Property CRUD and query operations.
    /// </summary>
    public interface IPropertyRepository
    {
        // CRUD
        Task<Property?> GetByIdAsync(int id);
        Task<Property?> GetByIdWithDetailsAsync(int id);
        Task<Property?> GetByWqfNumberAsync(string wqfNumber);
        Task<Property?> GetByLocalIdAsync(string localId);
        Task<IEnumerable<Property>> GetAllAsync();
        Task<Property> AddAsync(Property property);
        Task UpdateAsync(Property property);
        Task SoftDeleteAsync(int id);

        // Filtered queries
        IQueryable<Property> GetQueryable();
        Task<(IEnumerable<Property> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            int? governorateId = null,
            PropertyType? propertyType = null,
            OwnershipType? ownershipType = null,
            ApprovalStage? approvalStage = null,
            PropertyStatus? status = null,
            string? searchTerm = null,
            int? createdById = null);

        // GIS queries
        Task<IEnumerable<Property>> GetNearbyAsync(decimal latitude, decimal longitude, double radiusMeters, int limit = 20);
        Task<IEnumerable<Property>> GetByGisSyncStatusAsync(GisSyncStatus status);
        Task<IEnumerable<Property>> GetWithPendingGisSyncAsync();

        // Statistics
        Task<int> GetCountByGovernorateAsync(int governorateId);
        Task<int> GetCountByApprovalStageAsync(ApprovalStage stage);
        Task<Dictionary<int, int>> GetCountsByGovernorateAsync();
        Task<decimal> GetAverageDqsScoreAsync(int? governorateId = null);

        // Floors, Units, etc.
        Task<IEnumerable<PropertyFloor>> GetFloorsByPropertyIdAsync(int propertyId);
        Task<IEnumerable<PropertyUnit>> GetUnitsByPropertyIdAsync(int propertyId);
        Task<IEnumerable<PropertyUnit>> GetUnitsByFloorIdAsync(int floorId);
        Task<IEnumerable<PropertyDocument>> GetDocumentsByPropertyIdAsync(int propertyId);
        Task<IEnumerable<PropertyPhoto>> GetPhotosByPropertyIdAsync(int propertyId);
        Task<IEnumerable<PropertyPartnership>> GetPartnershipsByPropertyIdAsync(int propertyId);
        Task<AgriculturalDetail?> GetAgriculturalDetailByPropertyIdAsync(int propertyId);
        Task<IEnumerable<PropertyWorkflowHistory>> GetWorkflowHistoryAsync(int propertyId);

        // Map data
        Task<IEnumerable<Property>> GetMapPointsAsync(
            int? governorateId = null,
            PropertyType? propertyType = null,
            ApprovalStage? approvalStage = null);
    }
}
