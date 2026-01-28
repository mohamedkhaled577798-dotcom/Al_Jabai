using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة الدوائر الوقفية
/// </summary>
public class OfficeService
{
    private readonly IUnitOfWork _unitOfWork;

    public OfficeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WaqfOffice>> GetAllAsync()
    {
        return await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType)
            .Include(o => o.Province)
            .Include(o => o.ParentOffice)
            .OrderBy(o => o.OfficeType.Level)
            .ThenBy(o => o.NameAr)
            .ToListAsync();
    }

    public async Task<WaqfOffice?> GetByIdAsync(int id)
    {
        return await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType)
            .Include(o => o.Province)
            .Include(o => o.ParentOffice)
            .Include(o => o.ChildOffices)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<WaqfOffice> CreateAsync(WaqfOffice office)
    {
        office.Code = await GenerateCodeAsync();
        await _unitOfWork.WaqfOffices.AddAsync(office);
        await _unitOfWork.SaveChangesAsync();
        return office;
    }

    public async Task UpdateAsync(WaqfOffice office)
    {
        await _unitOfWork.WaqfOffices.UpdateAsync(office);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> GenerateCodeAsync()
    {
        var count = await _unitOfWork.WaqfOffices.CountAsync() + 1;
        return $"WQO-{count:D4}";
    }
}
