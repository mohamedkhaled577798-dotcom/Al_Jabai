using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IMobileSyncService
    {
        Task<SyncPushResponseDto> PushSyncAsync(SyncPushDto dto, int userId);
        Task<SyncPullResponseDto> PullSyncAsync(DateTime lastSyncAt, int userId, int? governorateId);
        Task<InitialSyncDto> GetInitialSyncDataAsync();
    }

    public class MobileSyncService : IMobileSyncService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPropertyService _propertyService;
        private readonly ILogger<MobileSyncService> _logger;

        public MobileSyncService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPropertyService propertyService,
            ILogger<MobileSyncService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _propertyService = propertyService;
            _logger = logger;
        }

        public async Task<SyncPushResponseDto> PushSyncAsync(SyncPushDto dto, int userId)
        {
            var response = new SyncPushResponseDto();

            // 1. Process Properties
            foreach (var item in dto.Properties)
            {
                try
                {
                    var propertyDto = JsonSerializer.Deserialize<CreatePropertyDto>(item.JsonData);
                    if (propertyDto == null) continue;

                    propertyDto.LocalId = item.LocalId; // Ensure localId is preserved
                    
                    PropertyDetailDto result;
                    if (item.ServerId.HasValue)
                    {
                        // Update existing
                        var updateDto = _mapper.Map<UpdatePropertyDto>(propertyDto);
                        updateDto.Id = item.ServerId.Value;
                        result = await _propertyService.UpdateAsync(updateDto, userId);
                    }
                    else
                    {
                        // Create new
                        result = await _propertyService.CreateAsync(propertyDto, userId);
                    }

                    response.SyncedIds.Add(new SyncedIdMapping { LocalId = item.LocalId, ServerId = result.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync property {LocalId}", item.LocalId);
                    response.Conflicts.Add(new SyncConflict { LocalId = item.LocalId, ConflictType = "Error", ServerVersion = ex.Message });
                }
            }

            // Note: Photos and Documents would need similar loop with file storage calls
            // For now, focusing on core property metadata sync

            return response;
        }

        public async Task<SyncPullResponseDto> PullSyncAsync(DateTime lastSyncAt, int userId, int? governorateId)
        {
            var query = _unitOfWork.Properties.GetQueryable()
                .Where(p => p.UpdatedAt > lastSyncAt || p.CreatedAt > lastSyncAt);

            // Scope by governorate for mobile users
            if (governorateId.HasValue)
                query = query.Where(p => p.GovernorateId == governorateId.Value);

            var items = await query.ToListAsync();

            return new SyncPullResponseDto
            {
                Properties = items.Select(p => new SyncPropertyItem
                {
                    ServerId = p.Id,
                    LocalId = p.LocalId ?? "",
                    JsonData = JsonSerializer.Serialize(_mapper.Map<PropertyDetailDto>(p)),
                    ModifiedAt = p.UpdatedAt ?? p.CreatedAt
                }).ToList(),
                ServerTime = DateTime.UtcNow
            };
        }

        public async Task<InitialSyncDto> GetInitialSyncDataAsync()
        {
            var data = new InitialSyncDto();

            data.Governorates = _mapper.Map<List<GovernorateItem>>(await _unitOfWork.GetQueryable<Governorate>().Where(x => !x.IsDeleted).ToListAsync());
            data.Districts = _mapper.Map<List<DistrictItem>>(await _unitOfWork.GetQueryable<District>().Where(x => !x.IsDeleted).ToListAsync());
            data.SubDistricts = _mapper.Map<List<SubDistrictItem>>(await _unitOfWork.GetQueryable<SubDistrict>().Where(x => !x.IsDeleted).ToListAsync());
            data.Neighborhoods = _mapper.Map<List<NeighborhoodItem>>(await _unitOfWork.GetQueryable<Neighborhood>().Where(x => !x.IsDeleted).ToListAsync());

            // Enums as lookups
            data.PropertyTypes = Enum.GetValues<PropertyType>()
                .Select(e => new LookupItem { Id = (int)e, NameAr = e.ToString() }).ToList();

            return data;
        }
    }
}
