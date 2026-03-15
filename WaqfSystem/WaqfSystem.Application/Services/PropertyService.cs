using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        public PropertyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PropertyService> logger, IDqsService dqsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dqsService = dqsService;
        }

        public async Task<PropertyDetailDto?> GetByIdAsync(int id)
        {
            var property = await _unitOfWork.Properties.GetByIdWithDetailsAsync(id);
            if (property == null) return null;
            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<PagedResult<PropertyListDto>> GetPagedAsync(PropertyFilterRequest filter, int? userId = null, string? userRole = null, int? governorateId = null)
        {
            // Role scoping
            int? scopedGovernorateId = filter.GovernorateId;
            int? scopedCreatedById = null;

            if (userRole == "REGIONAL_MGR" && governorateId.HasValue)
                scopedGovernorateId = governorateId;
            else if (userRole == "FIELD_INSPECTOR" && userId.HasValue)
                scopedCreatedById = userId;

            var (items, totalCount) = await _unitOfWork.Properties.GetPagedAsync(
                filter.Page, filter.PageSize,
                scopedGovernorateId, filter.PropertyType, filter.OwnershipType,
                filter.ApprovalStage, filter.Status, filter.SearchTerm,
                scopedCreatedById);

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

            await _unitOfWork.Properties.AddAsync(property);

            // Create address if provided
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
            }

            await _unitOfWork.SaveChangesAsync();

            // Calculate DQS
            property.DqsScore = _dqsService.CalculateScore(property);
            await _unitOfWork.Properties.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Property created: {WqfNumber} by user {UserId}", property.WqfNumber, userId);

            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<PropertyDetailDto> UpdateAsync(UpdatePropertyDto dto, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(dto.Id);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {dto.Id}");

            _mapper.Map(dto, property);
            property.UpdatedById = userId;
            property.UpdatedAt = DateTime.UtcNow;

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
