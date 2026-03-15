using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IGeographicService
    {
        Task<List<Governorate>> GetGovernoratesAsync(bool includeInactive = false);
        Task<List<District>> GetDistrictsAsync(int governorateId, bool includeInactive = false);
        Task<List<SubDistrict>> GetSubDistrictsAsync(int districtId, bool includeInactive = false);
        Task<List<Neighborhood>> GetNeighborhoodsAsync(int subDistrictId, bool includeInactive = false);
        Task<List<Street>> GetStreetsAsync(int neighborhoodId, bool includeInactive = false);
        void InvalidateGeographicCache();
        void InvalidateLevel(string level, int? parentId);
    }
}
