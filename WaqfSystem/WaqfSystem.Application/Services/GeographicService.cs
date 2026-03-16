using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class GeographicService : IGeographicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _geoKeys = new();
        private readonly object _syncObj = new();

        public GeographicService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
        }

        public async Task<List<Governorate>> GetGovernoratesAsync(bool includeInactive = false)
        {
            var cacheKey = BuildKey("governorate", includeInactive ? "all" : "active");
            if (_memoryCache.TryGetValue(cacheKey, out List<Governorate>? cached) && cached != null)
            {
                return cached;
            }

            var query = _unitOfWork.GetQueryable<Governorate>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.NameAr)
                .ToListAsync();

            SetCache(cacheKey, result);
            return result;
        }

        public async Task<List<District>> GetDistrictsAsync(int governorateId, bool includeInactive = false)
        {
            var cacheKey = BuildKey("district", governorateId, includeInactive ? "all" : "active");
            if (_memoryCache.TryGetValue(cacheKey, out List<District>? cached) && cached != null)
            {
                return cached;
            }

            var query = _unitOfWork.GetQueryable<District>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.GovernorateId == governorateId);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync();
            SetCache(cacheKey, result);
            return result;
        }

        public async Task<List<SubDistrict>> GetSubDistrictsAsync(int districtId, bool includeInactive = false)
        {
            var cacheKey = BuildKey("subdistrict", districtId, includeInactive ? "all" : "active");
            if (_memoryCache.TryGetValue(cacheKey, out List<SubDistrict>? cached) && cached != null)
            {
                return cached;
            }

            var query = _unitOfWork.GetQueryable<SubDistrict>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.DistrictId == districtId);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync();
            SetCache(cacheKey, result);
            return result;
        }

        public async Task<List<Neighborhood>> GetNeighborhoodsAsync(int subDistrictId, bool includeInactive = false)
        {
            var cacheKey = BuildKey("neighborhood", subDistrictId, includeInactive ? "all" : "active");
            if (_memoryCache.TryGetValue(cacheKey, out List<Neighborhood>? cached) && cached != null)
            {
                return cached;
            }

            var query = _unitOfWork.GetQueryable<Neighborhood>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.SubDistrictId == subDistrictId);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync();
            SetCache(cacheKey, result);
            return result;
        }

        public async Task<List<Street>> GetStreetsAsync(int neighborhoodId, bool includeInactive = false)
        {
            var cacheKey = BuildKey("street", neighborhoodId, includeInactive ? "all" : "active");
            if (_memoryCache.TryGetValue(cacheKey, out List<Street>? cached) && cached != null)
            {
                return cached;
            }

            var query = _unitOfWork.GetQueryable<Street>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.NeighborhoodId == neighborhoodId);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync();
            SetCache(cacheKey, result);
            return result;
        }

        public void InvalidateGeographicCache()
        {
            lock (_syncObj)
            {
                foreach (var key in _geoKeys.ToList())
                {
                    _memoryCache.Remove(key);
                }
                _geoKeys.Clear();
            }
        }

        public void InvalidateLevel(string level, int? parentId)
        {
            var keyActive = parentId.HasValue ? BuildKey(level, parentId.Value, "active") : BuildKey(level, "active");
            var keyAll = parentId.HasValue ? BuildKey(level, parentId.Value, "all") : BuildKey(level, "all");
            _memoryCache.Remove(keyActive);
            _memoryCache.Remove(keyAll);
            lock (_syncObj)
            {
                _geoKeys.Remove(keyActive);
                _geoKeys.Remove(keyAll);
            }
        }

        private void SetCache<T>(string key, T value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromHours(1));
            lock (_syncObj)
            {
                _geoKeys.Add(key);
            }
        }

        private static string BuildKey(string level, params object[] parts)
        {
            return $"geo_{level}_{string.Join("_", parts.Select(x => x.ToString()))}";
        }
    }
}
