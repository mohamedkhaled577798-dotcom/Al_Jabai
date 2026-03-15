using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ConcurrentDictionary<int, byte> _trackedRoleIds = new();

        public PermissionCacheService(IMemoryCache cache, IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<HashSet<string>> GetRolePermissionsAsync(int roleId)
        {
            var cacheKey = BuildRoleCacheKey(roleId);
            if (_cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached != null)
            {
                return cached;
            }

            var permissions = await _unitOfWork.GetQueryable<RolePermission>()
                .AsNoTracking()
                .Where(x => x.RoleId == roleId && x.Permission.IsActive)
                .Select(x => x.Permission.PermissionKey)
                .ToListAsync();

            var set = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            _cache.Set(cacheKey, set, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            _trackedRoleIds.TryAdd(roleId, 0);
            return set;
        }

        public void InvalidateRole(int roleId)
        {
            _cache.Remove(BuildRoleCacheKey(roleId));
            _trackedRoleIds.TryRemove(roleId, out _);
        }

        public void InvalidateAll()
        {
            foreach (var roleId in _trackedRoleIds.Keys.ToList())
            {
                _cache.Remove(BuildRoleCacheKey(roleId));
                _trackedRoleIds.TryRemove(roleId, out _);
            }
        }

        private static string BuildRoleCacheKey(int roleId) => $"role_permissions_{roleId}";
    }
}
