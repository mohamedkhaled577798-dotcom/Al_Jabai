using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IAgriculturalService
    {
        Task<AgriculturalDetailDto> UpsertAsync(CreateAgriculturalDto dto, int userId);
        Task<AgriculturalDetailDto?> GetByPropertyIdAsync(int propertyId);
        Task DeleteAsync(int propertyId);
    }

    public class AgriculturalService : IAgriculturalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AgriculturalService> _logger;

        public AgriculturalService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AgriculturalService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AgriculturalDetailDto> UpsertAsync(CreateAgriculturalDto dto, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(dto.PropertyId);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {dto.PropertyId}");

            var existing = await _unitOfWork.Properties.GetAgriculturalDetailByPropertyIdAsync(dto.PropertyId);
            AgriculturalDetail detail;

            if (existing != null)
            {
                _mapper.Map(dto, existing);
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedById = userId;
                detail = existing;
                await _unitOfWork.UpdateAsync(detail);
            }
            else
            {
                detail = _mapper.Map<AgriculturalDetail>(dto);
                detail.CreatedById = userId;
                detail.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.AddAsync(detail);
                
                // Update property type if it was different
                if (property.PropertyType != PropertyType.Agricultural)
                {
                    property.PropertyType = PropertyType.Agricultural;
                    await _unitOfWork.Properties.UpdateAsync(property);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Agricultural detail upserted for property {PropertyId}", dto.PropertyId);

            return _mapper.Map<AgriculturalDetailDto>(detail);
        }

        public async Task<AgriculturalDetailDto?> GetByPropertyIdAsync(int propertyId)
        {
            var detail = await _unitOfWork.Properties.GetAgriculturalDetailByPropertyIdAsync(propertyId);
            return detail == null ? null : _mapper.Map<AgriculturalDetailDto>(detail);
        }

        public async Task DeleteAsync(int propertyId)
        {
            var existing = await _unitOfWork.Properties.GetAgriculturalDetailByPropertyIdAsync(propertyId);
            if (existing != null)
            {
                await _unitOfWork.DeleteAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Agricultural detail deleted for property {PropertyId}", propertyId);
            }
        }
    }
}
