using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة المساجد
/// </summary>
public class MosqueService
{
    private readonly IUnitOfWork _unitOfWork;

    public MosqueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Mosque>> GetAllAsync()
    {
        return await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Include(m => m.Province)
            .Include(m => m.District)
            .Include(m => m.WaqfOffice)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Mosque?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Include(m => m.Province)
            .Include(m => m.District)
            .Include(m => m.SubDistrict)
            .Include(m => m.WaqfOffice)
            .Include(m => m.Images)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Mosque>> GetByProvinceAsync(int provinceId)
    {
        return await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Where(m => m.ProvinceId == provinceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mosque>> GetByOfficeAsync(int officeId)
    {
        return await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Where(m => m.WaqfOfficeId == officeId)
            .ToListAsync();
    }

    public async Task<Mosque> CreateAsync(Mosque mosque)
    {
        mosque.Code = await GenerateCodeAsync();
        await _unitOfWork.Mosques.AddAsync(mosque);
        await _unitOfWork.SaveChangesAsync();
        return mosque;
    }

    public async Task UpdateAsync(Mosque mosque)
    {
        await _unitOfWork.Mosques.UpdateAsync(mosque);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var mosque = await _unitOfWork.Mosques.GetByIdAsync(id);
        if (mosque != null)
        {
            await _unitOfWork.Mosques.DeleteAsync(mosque);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateCodeAsync()
    {
        var count = await _unitOfWork.Mosques.CountAsync() + 1;
        return $"MSQ-{count:D5}";
    }

    public async Task<IEnumerable<object>> GetForMapAsync()
    {
        return await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Select(m => new
            {
                m.Id,
                m.Code,
                m.NameAr,
                m.NameEn,
                Type = m.MosqueType.NameAr,
                TypeIcon = m.MosqueType.IconName,
                Status = m.MosqueStatus != null ? m.MosqueStatus.NameAr : "غير محدد",
                StatusColor = m.MosqueStatus != null ? m.MosqueStatus.ColorCode : "#6c757d",
                Latitude = m.Location.Y,
                Longitude = m.Location.X,
                m.Address,
                m.Capacity
            })
            .ToListAsync();
    }
}
