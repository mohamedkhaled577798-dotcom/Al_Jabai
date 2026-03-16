using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IPropertyService
    {
        Task<PropertyDetailDto?> GetByIdAsync(int id);
        Task<PagedResult<PropertyListDto>> GetPagedAsync(PropertyFilterRequest filter, int? userId = null, string? userRole = null, int? governorateId = null);
        Task<PropertyDetailDto> CreateAsync(CreatePropertyDto dto, int userId);
        Task<PropertyDetailDto> UpdateAsync(UpdatePropertyDto dto, int userId);
        Task SoftDeleteAsync(int id);
        Task DeactivateAsync(int id, int userId);
        Task<List<PropertyMapPointDto>> GetMapPointsAsync(int? governorateId = null, PropertyType? type = null, ApprovalStage? stage = null);
        Task<PropertyDetailDto?> GetByLocalIdAsync(string localId);
        Task<List<PropertyListDto>> GetNearbyAsync(decimal lat, decimal lng, double radius, int limit);
        string GenerateWqfNumber(int governorateId);
    }

    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PropertyService> _logger;
        private readonly IDqsService _dqsService;
        private readonly IGeographicScopeService _geographicScopeService;

        public PropertyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PropertyService> logger, IDqsService dqsService, IGeographicScopeService geographicScopeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dqsService = dqsService;
            _geographicScopeService = geographicScopeService;
        }

        public async Task<PropertyDetailDto?> GetByIdAsync(int id)
        {
            var property = await _unitOfWork.Properties.GetByIdWithDetailsAsync(id);
            if (property == null) return null;
            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<PagedResult<PropertyListDto>> GetPagedAsync(PropertyFilterRequest filter, int? userId = null, string? userRole = null, int? governorateId = null)
        {
            var query = _unitOfWork.Properties.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Governorate)
                .Include(p => p.CreatedBy)
                .AsQueryable();

            if (userId.HasValue)
            {
                var scope = await _geographicScopeService.BuildScopeAsync(userId.Value, userRole);
                query = _geographicScopeService.ApplyToProperties(query, scope);
            }

            if (userRole == "FIELD_INSPECTOR" && userId.HasValue)
            {
                query = query.Where(p => p.CreatedById == userId.Value);
            }

            if (filter.GovernorateId.HasValue)
                query = query.Where(p => p.GovernorateId == filter.GovernorateId.Value);

            if (filter.PropertyType.HasValue)
                query = query.Where(p => p.PropertyType == filter.PropertyType.Value);

            if (filter.OwnershipType.HasValue)
                query = query.Where(p => p.OwnershipType == filter.OwnershipType.Value);

            if (filter.ApprovalStage.HasValue)
                query = query.Where(p => p.ApprovalStage == filter.ApprovalStage.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.PropertyStatus == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(p =>
                    (p.PropertyName != null && p.PropertyName.Contains(term)) ||
                    p.WqfNumber.Contains(term) ||
                    (p.DeedNumber != null && p.DeedNumber.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<PropertyListDto>>(items);

            return new PagedResult<PropertyListDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PropertyDetailDto> CreateAsync(CreatePropertyDto dto, int userId)
        {
            var property = _mapper.Map<Core.Entities.Property>(dto);
            property.CreatedById = userId;
            property.CreatedAt = DateTime.UtcNow;
            property.ApprovalStage = ApprovalStage.Draft;
            property.PropertyStatus = PropertyStatus.Active;

            // Generate WQF number
            if (dto.GovernorateId.HasValue)
                property.WqfNumber = GenerateWqfNumber(dto.GovernorateId.Value);
            else
                property.WqfNumber = $"WQF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            // Idempotent — check for existing localId
            if (!string.IsNullOrEmpty(dto.LocalId))
            {
                var existing = await _unitOfWork.Properties.GetByLocalIdAsync(dto.LocalId);
                if (existing != null)
                {
                    _logger.LogInformation("Property with localId {LocalId} already exists, returning existing", dto.LocalId);
                    return _mapper.Map<PropertyDetailDto>(existing);
                }
            }

            // Save the property first so we get a valid DB-generated Id
            await _unitOfWork.Properties.AddAsync(property);
            await _unitOfWork.SaveChangesAsync();

            // Create address AFTER saving the property so PropertyId is valid
            if (dto.StreetId.HasValue || !string.IsNullOrEmpty(dto.BuildingNumber))
            {
                var address = new PropertyAddress
                {
                    PropertyId = property.Id,
                    StreetId = dto.StreetId,
                    BuildingNumber = dto.BuildingNumber,
                    PlotNumber = dto.PlotNumber,
                    BlockNumber = dto.BlockNumber,
                    NearestLandmark = dto.NearestLandmark,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();
                property.Address = address;
            }

            // Calculate DQS
            property.DqsScore = _dqsService.CalculateScore(property);
            await _unitOfWork.Properties.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Property created: {WqfNumber} by user {UserId}", property.WqfNumber, userId);

            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<PropertyDetailDto> UpdateAsync(UpdatePropertyDto dto, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdWithDetailsAsync(dto.Id);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {dto.Id}");

            _mapper.Map(dto, property);
            property.UpdatedById = userId;
            property.UpdatedAt = DateTime.UtcNow;

            var hasAddressData = dto.StreetId.HasValue
                || !string.IsNullOrWhiteSpace(dto.BuildingNumber)
                || !string.IsNullOrWhiteSpace(dto.PlotNumber)
                || !string.IsNullOrWhiteSpace(dto.BlockNumber)
                || !string.IsNullOrWhiteSpace(dto.NearestLandmark);

            if (property.Address == null && hasAddressData)
            {
                property.Address = new PropertyAddress
                {
                    PropertyId = property.Id,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            if (property.Address != null)
            {
                property.Address.StreetId = dto.StreetId;
                property.Address.BuildingNumber = dto.BuildingNumber;
                property.Address.PlotNumber = dto.PlotNumber;
                property.Address.BlockNumber = dto.BlockNumber;
                property.Address.NearestLandmark = dto.NearestLandmark;
                property.Address.UpdatedAt = DateTime.UtcNow;
                property.Address.UpdatedById = userId;
            }

            // Recalculate DQS
            property.DqsScore = _dqsService.CalculateScore(property);

            await _unitOfWork.Properties.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Property updated: {WqfNumber} by user {UserId}", property.WqfNumber, userId);

            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task SoftDeleteAsync(int id)
        {
            await _unitOfWork.Properties.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Property soft-deleted: {Id}", id);
        }

        public async Task DeactivateAsync(int id, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(id);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {id}");

            property.PropertyStatus = PropertyStatus.Abandoned;
            property.UpdatedById = userId;
            property.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Properties.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Property deactivated: {Id}", id);
        }

        public async Task<List<PropertyMapPointDto>> GetMapPointsAsync(int? governorateId = null, PropertyType? type = null, ApprovalStage? stage = null)
        {
            var properties = await _unitOfWork.Properties.GetMapPointsAsync(governorateId, type, stage);
            return _mapper.Map<List<PropertyMapPointDto>>(properties);
        }

        public async Task<PropertyDetailDto?> GetByLocalIdAsync(string localId)
        {
            var property = await _unitOfWork.Properties.GetByLocalIdAsync(localId);
            return property == null ? null : _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<List<PropertyListDto>> GetNearbyAsync(decimal lat, decimal lng, double radius, int limit)
        {
            var properties = await _unitOfWork.Properties.GetNearbyAsync(lat, lng, radius, limit);
            return _mapper.Map<List<PropertyListDto>>(properties.ToList());
        }

        public string GenerateWqfNumber(int governorateId)
        {
            var prefix = governorateId.ToString("D2");
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var unique = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"WQF-{prefix}-{timestamp}-{unique}";
        }
    }
}
