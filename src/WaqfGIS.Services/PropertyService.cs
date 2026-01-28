using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة العقارات
/// </summary>
public class PropertyService
{
    private readonly IUnitOfWork _unitOfWork;

    public PropertyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WaqfProperty>> GetAllAsync()
    {
        return await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .Include(p => p.UsageType)
            .Include(p => p.Province)
            .Include(p => p.District)
            .Include(p => p.WaqfOffice)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<WaqfProperty?> GetByIdAsync(int id)
    {
        return await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .Include(p => p.UsageType)
            .Include(p => p.Province)
            .Include(p => p.District)
            .Include(p => p.WaqfOffice)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<WaqfProperty> CreateAsync(WaqfProperty property)
    {
        property.Code = await GenerateCodeAsync();
        await _unitOfWork.WaqfProperties.AddAsync(property);
        await _unitOfWork.SaveChangesAsync();
        return property;
    }

    public async Task UpdateAsync(WaqfProperty property)
    {
        await _unitOfWork.WaqfProperties.UpdateAsync(property);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var property = await _unitOfWork.WaqfProperties.GetByIdAsync(id);
        if (property != null)
        {
            await _unitOfWork.WaqfProperties.DeleteAsync(property);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateCodeAsync()
    {
        var count = await _unitOfWork.WaqfProperties.CountAsync() + 1;
        return $"WQP-{count:D5}";
    }

    public async Task<IEnumerable<object>> GetForMapAsync()
    {
        return await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .Include(p => p.UsageType)
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.NameAr,
                Type = p.PropertyType.NameAr,
                TypeIcon = p.PropertyType.IconName,
                Usage = p.UsageType != null ? p.UsageType.NameAr : "غير محدد",
                Latitude = p.Location.Y,
                Longitude = p.Location.X,
                p.Address,
                p.AreaSqm,
                p.EstimatedValue
            })
            .ToListAsync();
    }
}
