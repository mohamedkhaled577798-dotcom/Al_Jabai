using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Admin;
using WaqfSystem.Application.DTOs.Common;

namespace WaqfSystem.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardDto Dashboard { get; set; } = new();
        public List<AuditLogDto> RecentActivity { get; set; } = new();
    }

    public class RolesViewModel
    {
        public List<RoleDetailDto> Roles { get; set; } = new();
        public int TotalSystemRoles { get; set; }
        public int TotalCustomRoles { get; set; }
    }

    public class CreateEditRoleViewModel
    {
        public CreateRoleDto Role { get; set; } = new();
        public bool IsEditMode { get; set; }
        public List<PermissionGroupDto> PermissionGroups { get; set; } = new();
        public HashSet<int> SelectedPermissionIds { get; set; } = new();
        public List<string> AvailableIcons { get; set; } = new();
        public List<string> AvailableColors { get; set; } = new();
        public int RoleId { get; set; }
    }

    public class UsersViewModel
    {
        public PagedResult<UserListItemDto> Users { get; set; } = new();
        public UserFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public List<RoleDetailDto> RoleDetails { get; set; } = new();
    }

    public class CreateEditUserViewModel
    {
        public CreateUserDto User { get; set; } = new();
        public bool IsEditMode { get; set; }
        public int UserId { get; set; }
        public List<SelectListItem> Roles { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public List<SelectListItem> Districts { get; set; } = new();
        public List<SelectListItem> SubDistricts { get; set; } = new();
        public Dictionary<int, int> RoleScopeLevels { get; set; } = new();
        public HashSet<int> GlobalScopeRoleIds { get; set; } = new();
        public List<int> SelectedGovernorateIds { get; set; } = new();
        public List<int> SelectedDistrictIds { get; set; } = new();
        public List<int> SelectedSubDistrictIds { get; set; } = new();
    }

    public class UserDetailViewModel
    {
        public UserDetailDto User { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
        public List<PermissionGroupDto> PermissionGroups { get; set; } = new();
        public PagedResult<AuditLogDto> UserAuditLogs { get; set; } = new();
    }

    public class GeographicCountsViewModel
    {
        public int Districts { get; set; }
        public int SubDistricts { get; set; }
        public int Neighborhoods { get; set; }
        public int Streets { get; set; }
    }

    public class GeographicViewModel
    {
        public List<GovernorateDto> Governorates { get; set; } = new();
        public GeographicCountsViewModel TotalCounts { get; set; } = new();
    }

    public class AuditLogViewModel
    {
        public PagedResult<AuditLogDto> Logs { get; set; } = new();
        public AuditLogFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> EntityTypes { get; set; } = new();
    }
}
