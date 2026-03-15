using System.Collections.Generic;
using System.Threading.Tasks;

namespace WaqfSystem.Application.Services
{
    public interface IPermissionCacheService
    {
        Task<HashSet<string>> GetRolePermissionsAsync(int roleId);
        void InvalidateRole(int roleId);
        void InvalidateAll();
    }
}
