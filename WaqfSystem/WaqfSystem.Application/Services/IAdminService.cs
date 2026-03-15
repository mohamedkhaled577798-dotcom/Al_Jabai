using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Admin;
using WaqfSystem.Application.DTOs.Common;

namespace WaqfSystem.Application.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();

        Task<List<RoleDetailDto>> GetAllRolesAsync();
        Task<RoleDetailDto?> GetRoleByIdAsync(int id);
        Task<int> CreateRoleAsync(CreateRoleDto dto, int userId);
        Task UpdateRoleAsync(UpdateRoleDto dto, int userId);
        Task DeleteRoleAsync(int id, int userId);
        Task SetRolePermissionsAsync(int roleId, List<int> permissionIds, int userId);
        Task<List<PermissionGroupDto>> GetAllPermissionsGroupedAsync();

        Task<PagedResult<UserListItemDto>> GetUsersAsync(UserFilterRequest filter);
        Task<UserDetailDto?> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(CreateUserDto dto, int userId);
        Task UpdateUserAsync(UpdateUserDto dto, int userId);
        Task ChangePasswordAsync(int userId, string newPassword, int changedById);
        Task ActivateUserAsync(int id, int userId);
        Task DeactivateUserAsync(int id, string reason, int userId);
        Task LockUserAsync(int id, string reason, int userId);
        Task UnlockUserAsync(int id, int userId);
        Task AssignRoleAsync(int userId, int roleId, int assignedById);
        Task AssignUserRoleAsync(AssignUserRoleDto dto, int assignedById);
        Task<UserGeographicScopeSelectionDto> GetUserScopeSelectionAsync(int userId);
        Task<string> ResetPasswordAsync(int userId, int adminId);

        Task<List<GovernorateDto>> GetAllGovernoratesAsync(bool includeInactive = false);
        Task<GovernorateDto?> GetGovernorateAsync(int id);
        Task<int> CreateGovernorateAsync(CreateGovernorateDto dto, int userId);
        Task UpdateGovernorateAsync(UpdateGovernorateDto dto, int userId);
        Task ToggleGovernorateAsync(int id, int userId);

        Task<List<DistrictDto>> GetDistrictsAsync(int governorateId, bool includeInactive = false);
        Task<int> CreateDistrictAsync(CreateDistrictDto dto, int userId);
        Task UpdateDistrictAsync(UpdateDistrictDto dto, int userId);
        Task ToggleDistrictAsync(int id, int userId);

        Task<List<SubDistrictDto>> GetSubDistrictsAsync(int districtId, bool includeInactive = false);
        Task<int> CreateSubDistrictAsync(CreateSubDistrictDto dto, int userId);
        Task UpdateSubDistrictAsync(UpdateSubDistrictDto dto, int userId);
        Task ToggleSubDistrictAsync(int id, int userId);

        Task<List<NeighborhoodDto>> GetNeighborhoodsAsync(int subDistrictId, bool includeInactive = false);
        Task<int> CreateNeighborhoodAsync(CreateNeighborhoodDto dto, int userId);
        Task UpdateNeighborhoodAsync(UpdateNeighborhoodDto dto, int userId);
        Task ToggleNeighborhoodAsync(int id, int userId);

        Task<List<StreetDto>> GetStreetsAsync(int neighborhoodId, bool includeInactive = false);
        Task<int> CreateStreetAsync(CreateStreetDto dto, int userId);
        Task UpdateStreetAsync(UpdateStreetDto dto, int userId);
        Task ToggleStreetAsync(int id, int userId);

        Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterRequest filter);
    }
}
