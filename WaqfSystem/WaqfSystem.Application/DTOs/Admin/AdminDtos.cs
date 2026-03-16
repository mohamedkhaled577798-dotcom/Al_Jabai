using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Admin
{
    public class RoleDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayNameAr { get; set; }
        public string? DisplayNameEn { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public GeographicScopeLevel GeographicScopeLevel { get; set; }
        public bool HasGlobalScope { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRoleDto
    {
        [Required]
        [RegularExpression("^[A-Z_]+$")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string DisplayNameAr { get; set; } = string.Empty;

        public string? DisplayNameEn { get; set; }
        public string? Description { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$")]
        public string? Color { get; set; }

        public string? Icon { get; set; }
        public GeographicScopeLevel GeographicScopeLevel { get; set; } = GeographicScopeLevel.None;
        public bool HasGlobalScope { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [RegularExpression("^[A-Z_]+$")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string DisplayNameAr { get; set; } = string.Empty;

        public string? DisplayNameEn { get; set; }
        public string? Description { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$")]
        public string? Color { get; set; }

        public string? Icon { get; set; }
        public GeographicScopeLevel GeographicScopeLevel { get; set; } = GeographicScopeLevel.None;
        public bool HasGlobalScope { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PermissionGroupDto
    {
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleDisplayAr { get; set; } = string.Empty;
        public List<PermissionItemDto> Permissions { get; set; } = new();
    }

    public class PermissionItemDto
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string DisplayNameAr { get; set; } = string.Empty;
        public string DisplayNameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SetRolePermissionsDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UserListItemDto
    {
        public int Id { get; set; }
        public string FullNameAr { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDisplayNameAr { get; set; } = string.Empty;
        public string? RoleColor { get; set; }
        public string? RoleIcon { get; set; }
        public string? GovernorateNameAr { get; set; }
        public string? DistrictNameAr { get; set; }
        public string? SubDistrictNameAr { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActiveMissionsCount { get; set; }
    }

    public class UserDetailDto : UserListItemDto
    {
        public string? NationalId { get; set; }
        public string? Phone2 { get; set; }
        public string? Notes { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public bool MustChangePassword { get; set; }
        public int FailedLoginCount { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CreatedByName { get; set; }
        public string? LastLoginIp { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string FullNameAr { get; set; } = string.Empty;

        public string? FullNameEn { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Phone2 { get; set; }
        public string? NationalId { get; set; }
        public string? JobTitle { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public List<int> GovernorateIds { get; set; } = new();
        public List<int> DistrictIds { get; set; } = new();
        public List<int> SubDistrictIds { get; set; } = new();
        public string? Notes { get; set; }
        public bool MustChangePassword { get; set; } = true;
        public string? ProfilePhotoUrl { get; set; }
    }

    public class UpdateUserDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string FullNameAr { get; set; } = string.Empty;

        public string? FullNameEn { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Phone2 { get; set; }
        public string? NationalId { get; set; }
        public string? JobTitle { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public List<int> GovernorateIds { get; set; } = new();
        public List<int> DistrictIds { get; set; } = new();
        public List<int> SubDistrictIds { get; set; } = new();
        public string? Notes { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ProfilePhotoUrl { get; set; }
    }

    public class UserFilterRequest
    {
        public string? SearchTerm { get; set; }
        public int? RoleId { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserGeographicScopeSelectionDto
    {
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public List<int> GovernorateIds { get; set; } = new();
        public List<int> DistrictIds { get; set; } = new();
        public List<int> SubDistrictIds { get; set; } = new();
    }

    public class AssignUserRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public List<int> GovernorateIds { get; set; } = new();
        public List<int> DistrictIds { get; set; } = new();
        public List<int> SubDistrictIds { get; set; } = new();
    }

    public class GovernorateDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int DistrictCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateGovernorateDto
    {
        [Required] public string NameAr { get; set; } = string.Empty;
        [Required] public string NameEn { get; set; } = string.Empty;
        [Required] public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public string? PostalCode { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateGovernorateDto : CreateGovernorateDto
    {
        [Required] public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DistrictDto
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string GovernorateNameAr { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int SubDistrictCount { get; set; }
    }

    public class CreateDistrictDto
    {
        [Required] public int GovernorateId { get; set; }
        [Required] public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string? GisLayerId { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateDistrictDto : CreateDistrictDto
    {
        [Required] public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SubDistrictDto
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string DistrictNameAr { get; set; } = string.Empty;
        public string GovernorateNameAr { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int NeighborhoodCount { get; set; }
    }

    public class CreateSubDistrictDto
    {
        [Required] public int DistrictId { get; set; }
        [Required] public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string? GisLayerId { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateSubDistrictDto : CreateSubDistrictDto
    {
        [Required] public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class NeighborhoodDto
    {
        public int Id { get; set; }
        public int SubDistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "City";
        public string? PostalCode { get; set; }
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int StreetCount { get; set; }
    }

    public class CreateNeighborhoodDto
    {
        [Required] public int SubDistrictId { get; set; }
        [Required] public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string Type { get; set; } = "City";
        public string? PostalCode { get; set; }
        public string? GisLayerId { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateNeighborhoodDto : CreateNeighborhoodDto
    {
        [Required] public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class StreetDto
    {
        public int Id { get; set; }
        public int NeighborhoodId { get; set; }
        public string NeighborhoodNameAr { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLineId { get; set; }
        public string? StreetType { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateStreetDto
    {
        [Required] public int NeighborhoodId { get; set; }
        [Required] public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string? GisLineId { get; set; }
        public string? StreetType { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateStreetDto : CreateStreetDto
    {
        [Required] public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AuditLogDto
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuditLogFilterRequest
    {
        public int? UserId { get; set; }
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int TotalRoles { get; set; }
        public int CustomRoles { get; set; }
        public int TotalGovernates { get; set; }
        public int TotalDistricts { get; set; }
        public int TotalSubDistricts { get; set; }
        public int TotalNeighborhoods { get; set; }
        public int TotalStreets { get; set; }
        public List<AuditLogDto> RecentAuditLogs { get; set; } = new();
        public Dictionary<string, int> UsersByRole { get; set; } = new();
    }
}
