using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Revenue;

namespace WaqfSystem.Application.Services
{
    public interface IPropertyStructureService
    {
        Task<PropertyStructureDto> GetStructureAsync(long propertyId, string periodLabel, int userId);
    }
}
