using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Admin;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly IGeographicService _geographicService;

        public AdminService(IUnitOfWork unitOfWork, IPermissionCacheService permissionCacheService, IGeographicService geographicService)
        {
            _unitOfWork = unitOfWork;
            _permissionCacheService = permissionCacheService;
            _geographicService = geographicService;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var totalUsers = await _unitOfWork.GetQueryable<User>().AsNoTracking().CountAsync(x => !x.IsDeleted);
            var activeUsers = await _unitOfWork.GetQueryable<User>().AsNoTracking().CountAsync(x => !x.IsDeleted && x.IsActive);
            var lockedUsers = await _unitOfWork.GetQueryable<User>().AsNoTracking().CountAsync(x => !x.IsDeleted && x.IsLocked);
            var totalRoles = await _unitOfWork.GetQueryable<Role>().AsNoTracking().CountAsync(x => !x.IsDeleted);
            var customRoles = await _unitOfWork.GetQueryable<Role>().AsNoTracking().CountAsync(x => !x.IsDeleted && !x.IsSystemRole);

            var usersByRole = await _unitOfWork.GetQueryable<User>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Role.DisplayNameAr ?? x.Role.NameAr)
                .Select(x => new { Role = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);

            var recentAudit = await _unitOfWork.GetQueryable<AuditLog>()
                .AsNoTracking()
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .Select(x => new AuditLogDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User != null ? x.User.FullNameAr : "-",
                    Action = x.Action,
                    EntityType = x.TableName,
                    EntityId = x.RecordId,
                    OldValues = x.OldValues,
                    NewValues = x.NewValues,
                    IpAddress = x.IpAddress,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                LockedUsers = lockedUsers,
                TotalRoles = totalRoles,
                CustomRoles = customRoles,
                TotalGovernates = await _unitOfWork.GetQueryable<Governorate>().AsNoTracking().CountAsync(x => !x.IsDeleted),
                TotalDistricts = await _unitOfWork.GetQueryable<District>().AsNoTracking().CountAsync(x => !x.IsDeleted),
                TotalSubDistricts = await _unitOfWork.GetQueryable<SubDistrict>().AsNoTracking().CountAsync(x => !x.IsDeleted),
                TotalNeighborhoods = await _unitOfWork.GetQueryable<Neighborhood>().AsNoTracking().CountAsync(x => !x.IsDeleted),
                TotalStreets = await _unitOfWork.GetQueryable<Street>().AsNoTracking().CountAsync(x => !x.IsDeleted),
                RecentAuditLogs = recentAudit,
                UsersByRole = usersByRole
            };
        }

        public async Task<List<RoleDetailDto>> GetAllRolesAsync()
        {
            return await _unitOfWork.GetQueryable<Role>()
                .AsNoTracking()
                .Include(x => x.Users)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.IsSystemRole ? 0 : 1)
                .ThenBy(x => x.DisplayNameAr ?? x.NameAr)
                .Select(x => new RoleDetailDto
                {
                    Id = x.Id,
                    Name = x.Code,
                    DisplayNameAr = x.DisplayNameAr ?? x.NameAr,
                    DisplayNameEn = x.DisplayNameEn ?? x.NameEn,
                    Description = x.Description,
                    IsSystemRole = x.IsSystemRole,
                    IsActive = x.IsActive,
                    Color = x.Color,
                    Icon = x.Icon,
                    GeographicScopeLevel = x.GeographicScopeLevel,
                    HasGlobalScope = x.HasGlobalScope,
                    UserCount = x.Users.Count(u => !u.IsDeleted && u.IsActive),
                    PermissionCount = x.Permissions.Count,
                    Permissions = x.Permissions.Select(p => p.Permission.PermissionKey).ToList(),
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<RoleDetailDto?> GetRoleByIdAsync(int id)
        {
            return await _unitOfWork.GetQueryable<Role>()
                .AsNoTracking()
                .Include(x => x.Users)
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new RoleDetailDto
                {
                    Id = x.Id,
                    Name = x.Code,
                    DisplayNameAr = x.DisplayNameAr ?? x.NameAr,
                    DisplayNameEn = x.DisplayNameEn ?? x.NameEn,
                    Description = x.Description,
                    IsSystemRole = x.IsSystemRole,
                    IsActive = x.IsActive,
                    Color = x.Color,
                    Icon = x.Icon,
                    GeographicScopeLevel = x.GeographicScopeLevel,
                    HasGlobalScope = x.HasGlobalScope,
                    UserCount = x.Users.Count(u => !u.IsDeleted && u.IsActive),
                    PermissionCount = x.Permissions.Count,
                    Permissions = x.Permissions.Select(p => p.Permission.PermissionKey).ToList(),
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateRoleAsync(CreateRoleDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new InvalidOperationException("اسم الدور مطلوب");
            if (!Regex.IsMatch(dto.Name, "^[A-Z_]+$")) throw new InvalidOperationException("صيغة اسم الدور غير صحيحة");
            if (!string.IsNullOrWhiteSpace(dto.Color) && !Regex.IsMatch(dto.Color, "^#[0-9A-Fa-f]{6}$")) throw new InvalidOperationException("لون الدور غير صالح");

            var name = dto.Name.Trim();
            var exists = await _unitOfWork.GetQueryable<Role>().AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Code == name);
            if (exists) throw new InvalidOperationException("اسم الدور مستخدم مسبقًا");

            var role = new Role
            {
                Code = name,
                Name = name,
                NameAr = dto.DisplayNameAr,
                NameEn = dto.DisplayNameEn ?? name,
                DisplayNameAr = dto.DisplayNameAr,
                DisplayNameEn = dto.DisplayNameEn,
                Description = dto.Description,
                Color = dto.Color,
                Icon = dto.Icon,
                GeographicScopeLevel = dto.GeographicScopeLevel,
                HasGlobalScope = dto.HasGlobalScope,
                IsSystemRole = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _unitOfWork.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(userId, "Roles", role.Id, "CREATE", null, $"{{\"Code\":\"{role.Code}\"}}");
            return role.Id;
        }

        public async Task UpdateRoleAsync(UpdateRoleDto dto, int userId)
        {
            var role = await _unitOfWork.GetByIdAsync<Role>(dto.Id) ?? throw new InvalidOperationException("الدور غير موجود");
            if (role.IsDeleted) throw new InvalidOperationException("الدور غير موجود");

            if (role.IsSystemRole)
            {
                role.DisplayNameAr = dto.DisplayNameAr;
                role.DisplayNameEn = dto.DisplayNameEn;
                role.Description = dto.Description;
                role.Color = dto.Color;
                role.Icon = dto.Icon;
                role.GeographicScopeLevel = dto.GeographicScopeLevel;
                role.HasGlobalScope = dto.HasGlobalScope;
                role.IsActive = dto.IsActive;
            }
            else
            {
                if (!string.Equals(role.Code, dto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    role.Code = dto.Name;
                    role.Name = dto.Name;
                }

                role.NameAr = dto.DisplayNameAr;
                role.NameEn = dto.DisplayNameEn ?? dto.Name;
                role.DisplayNameAr = dto.DisplayNameAr;
                role.DisplayNameEn = dto.DisplayNameEn;
                role.Description = dto.Description;
                role.Color = dto.Color;
                role.Icon = dto.Icon;
                role.GeographicScopeLevel = dto.GeographicScopeLevel;
                role.HasGlobalScope = dto.HasGlobalScope;
                role.IsActive = dto.IsActive;
            }

            role.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
            _permissionCacheService.InvalidateRole(role.Id);
            await WriteAudit(userId, "Roles", role.Id, "UPDATE", null, $"{{\"Code\":\"{role.Code}\"}}");
        }

        public async Task DeleteRoleAsync(int id, int userId)
        {
            var role = await _unitOfWork.GetByIdAsync<Role>(id) ?? throw new InvalidOperationException("الدور غير موجود");
            if (role.IsSystemRole) throw new InvalidOperationException("لا يمكن حذف دور نظامي");

            var activeUsers = await _unitOfWork.GetQueryable<User>().AsNoTracking().AnyAsync(x => !x.IsDeleted && x.IsActive && x.RoleId == id);
            if (activeUsers) throw new InvalidOperationException("يوجد مستخدمون نشطون مرتبطون بهذا الدور");

            role.IsDeleted = true;
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
            _permissionCacheService.InvalidateRole(id);
            await WriteAudit(userId, "Roles", id, "DELETE", null, $"{{\"Code\":\"{role.Code}\"}}");
        }

        public async Task SetRolePermissionsAsync(int roleId, List<int> permissionIds, int userId)
        {
            var role = await _unitOfWork.GetByIdAsync<Role>(roleId) ?? throw new InvalidOperationException("الدور غير موجود");
            if (string.Equals(role.Code, "SYS_ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var existing = await _unitOfWork.GetQueryable<RolePermission>().Where(x => x.RoleId == roleId).ToListAsync();
            foreach (var item in existing)
            {
                await _unitOfWork.DeleteAsync(item);
            }

            foreach (var permissionId in permissionIds.Distinct())
            {
                await _unitOfWork.AddAsync(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    GrantedById = userId,
                    GrantedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _permissionCacheService.InvalidateRole(roleId);
            await WriteAudit(userId, "RolePermissions", roleId, "UPDATE", null, $"{{\"Role\":\"{role.Code}\",\"Permissions\":{permissionIds.Count}}}");
        }

        public async Task<List<PermissionGroupDto>> GetAllPermissionsGroupedAsync()
        {
            var list = await _unitOfWork.GetQueryable<Permission>()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Module)
                .ThenBy(x => x.DisplayNameAr)
                .Select(x => new PermissionItemDto
                {
                    Id = x.Id,
                    PermissionKey = x.PermissionKey,
                    DisplayNameAr = x.DisplayNameAr,
                    DisplayNameEn = x.DisplayNameEn,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return list
                .GroupBy(x => x.PermissionKey.Split('.')[0])
                .Select(g => new PermissionGroupDto
                {
                    ModuleName = g.Key,
                    ModuleDisplayAr = g.Key,
                    Permissions = g.ToList()
                })
                .OrderBy(x => x.ModuleName)
                .ToList();
        }

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserFilterRequest filter)
        {
            var query = _unitOfWork.GetQueryable<User>()
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Governorate)
                .Include(x => x.District)
                .Include(x => x.SubDistrict)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(x => x.FullNameAr.Contains(term) || x.Email.Contains(term));
            }

            if (filter.RoleId.HasValue) query = query.Where(x => x.RoleId == filter.RoleId.Value);
            if (filter.GovernorateId.HasValue) query = query.Where(x => x.GovernorateId == filter.GovernorateId.Value);
            if (filter.DistrictId.HasValue) query = query.Where(x => x.DistrictId == filter.DistrictId.Value);
            if (filter.SubDistrictId.HasValue) query = query.Where(x => x.SubDistrictId == filter.SubDistrictId.Value);
            if (filter.IsActive.HasValue) query = query.Where(x => x.IsActive == filter.IsActive.Value);
            if (filter.IsLocked.HasValue) query = query.Where(x => x.IsLocked == filter.IsLocked.Value);
            if (filter.CreatedFrom.HasValue) query = query.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue) query = query.Where(x => x.CreatedAt <= filter.CreatedTo.Value);

            var total = await query.CountAsync();
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var size = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new UserListItemDto
                {
                    Id = x.Id,
                    FullNameAr = x.FullNameAr,
                    FullNameEn = x.FullNameEn,
                    Email = x.Email,
                    Phone = x.Phone ?? x.PhoneNumber,
                    RoleName = x.Role.Code,
                    RoleDisplayNameAr = x.Role.DisplayNameAr ?? x.Role.NameAr,
                    RoleColor = x.Role.Color,
                    RoleIcon = x.Role.Icon,
                    GovernorateNameAr = x.Governorate != null ? x.Governorate.NameAr : null,
                    DistrictNameAr = x.District != null ? x.District.NameAr : null,
                    SubDistrictNameAr = x.SubDistrict != null ? x.SubDistrict.NameAr : null,
                    GovernorateId = x.GovernorateId,
                    DistrictId = x.DistrictId,
                    SubDistrictId = x.SubDistrictId,
                    JobTitle = x.JobTitle,
                    IsActive = x.IsActive,
                    IsLocked = x.IsLocked,
                    LastLoginAt = x.LastLoginAt,
                    CreatedAt = x.CreatedAt,
                    ActiveMissionsCount = _unitOfWork.GetQueryable<InspectionMission>().Count(m => !m.IsDeleted && m.AssignedToUserId == x.Id)
                })
                .ToListAsync();

            return new PagedResult<UserListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = size
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.GetQueryable<User>()
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Governorate)
                .Include(x => x.District)
                .Include(x => x.SubDistrict)
                .Include(x => x.CreatedBy)
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new UserDetailDto
                {
                    Id = x.Id,
                    FullNameAr = x.FullNameAr,
                    FullNameEn = x.FullNameEn,
                    Email = x.Email,
                    Phone = x.Phone ?? x.PhoneNumber,
                    Phone2 = x.Phone2,
                    NationalId = x.NationalId,
                    RoleName = x.Role.Code,
                    RoleDisplayNameAr = x.Role.DisplayNameAr ?? x.Role.NameAr,
                    RoleColor = x.Role.Color,
                    RoleIcon = x.Role.Icon,
                    GovernorateNameAr = x.Governorate != null ? x.Governorate.NameAr : null,
                    DistrictNameAr = x.District != null ? x.District.NameAr : null,
                    SubDistrictNameAr = x.SubDistrict != null ? x.SubDistrict.NameAr : null,
                    JobTitle = x.JobTitle,
                    IsActive = x.IsActive,
                    IsLocked = x.IsLocked,
                    LastLoginAt = x.LastLoginAt,
                    CreatedAt = x.CreatedAt,
                    ActiveMissionsCount = 0,
                    Notes = x.Notes,
                    PasswordChangedAt = x.PasswordChangedAt,
                    MustChangePassword = x.MustChangePassword,
                    FailedLoginCount = x.FailedLoginCount,
                    ProfilePhotoUrl = x.ProfilePhotoUrl,
                    CreatedByName = x.CreatedBy != null ? x.CreatedBy.FullNameAr : null,
                    LastLoginIp = x.LastLoginIp,
                    GovernorateId = x.GovernorateId,
                    DistrictId = x.DistrictId,
                    SubDistrictId = x.SubDistrictId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateUserAsync(CreateUserDto dto, int userId)
        {
            var emailExists = await _unitOfWork.GetQueryable<User>().AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Email == dto.Email);
            if (emailExists) throw new InvalidOperationException("البريد الإلكتروني مستخدم مسبقًا");

            var role = await _unitOfWork.GetQueryable<Role>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.RoleId && !x.IsDeleted)
                ?? throw new InvalidOperationException("الدور غير موجود");

            var selection = await NormalizeScopeSelectionAsync(
                role,
                dto.GovernorateId,
                dto.DistrictId,
                dto.SubDistrictId,
                dto.GovernorateIds,
                dto.DistrictIds,
                dto.SubDistrictIds);

            var entity = new User
            {
                FullNameAr = dto.FullNameAr,
                FullNameEn = dto.FullNameEn,
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                PhoneNumber = dto.Phone,
                Phone2 = dto.Phone2,
                NationalId = dto.NationalId,
                JobTitle = dto.JobTitle,
                RoleId = dto.RoleId,
                GovernorateId = selection.GovernorateId,
                DistrictId = selection.DistrictId,
                SubDistrictId = selection.SubDistrictId,
                Notes = dto.Notes,
                MustChangePassword = dto.MustChangePassword,
                ProfilePhotoUrl = dto.ProfilePhotoUrl,
                IsActive = true,
                IsLocked = false,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                PasswordChangedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await SyncUserScopesAsync(entity.Id, selection, userId);
            await WriteAudit(userId, "Users", entity.Id, "CREATE", null, $"{{\"Email\":\"{entity.Email}\"}}");
            return entity.Id;
        }

        public async Task UpdateUserAsync(UpdateUserDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<User>(dto.Id) ?? throw new InvalidOperationException("المستخدم غير موجود");
            var oldRoleId = entity.RoleId;
            var role = await _unitOfWork.GetQueryable<Role>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.RoleId && !x.IsDeleted)
                ?? throw new InvalidOperationException("الدور غير موجود");

            var selection = await NormalizeScopeSelectionAsync(
                role,
                dto.GovernorateId,
                dto.DistrictId,
                dto.SubDistrictId,
                dto.GovernorateIds,
                dto.DistrictIds,
                dto.SubDistrictIds);

            entity.FullNameAr = dto.FullNameAr;
            entity.FullNameEn = dto.FullNameEn;
            entity.Email = dto.Email.Trim();
            entity.Phone = dto.Phone;
            entity.PhoneNumber = dto.Phone;
            entity.Phone2 = dto.Phone2;
            entity.NationalId = dto.NationalId;
            entity.JobTitle = dto.JobTitle;
            entity.RoleId = dto.RoleId;
            entity.GovernorateId = selection.GovernorateId;
            entity.DistrictId = selection.DistrictId;
            entity.SubDistrictId = selection.SubDistrictId;
            entity.Notes = dto.Notes;
            entity.MustChangePassword = dto.MustChangePassword;
            entity.ProfilePhotoUrl = dto.ProfilePhotoUrl;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await SyncUserScopesAsync(entity.Id, selection, userId);

            if (oldRoleId != entity.RoleId)
            {
                _permissionCacheService.InvalidateRole(oldRoleId);
                _permissionCacheService.InvalidateRole(entity.RoleId);
            }

            await WriteAudit(userId, "Users", entity.Id, "UPDATE", null, $"{{\"Email\":\"{entity.Email}\"}}");
        }

        public async Task ChangePasswordAsync(int userId, string newPassword, int changedById)
        {
            var entity = await _unitOfWork.GetByIdAsync<User>(userId) ?? throw new InvalidOperationException("المستخدم غير موجود");
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            entity.PasswordChangedAt = DateTime.UtcNow;
            entity.MustChangePassword = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(changedById, "Users", entity.Id, "PASSWORD_CHANGE", null, "{}");
        }

        public async Task ActivateUserAsync(int id, int userId)
        {
            if (id == userId) throw new InvalidOperationException("لا يمكنك تعديل حالة حسابك بهذه العملية");
            var user = await _unitOfWork.GetByIdAsync<User>(id) ?? throw new InvalidOperationException("المستخدم غير موجود");
            user.IsActive = true;
            user.IsLocked = false;
            user.FailedLoginCount = 0;
            user.LockReason = null;
            user.LockedAt = null;
            user.LockedById = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(userId, "Users", id, "ACTIVATE", null, null);
        }

        public async Task DeactivateUserAsync(int id, string reason, int userId)
        {
            if (id == userId) throw new InvalidOperationException("لا يمكنك إيقاف حسابك");
            var user = await _unitOfWork.GetByIdAsync<User>(id) ?? throw new InvalidOperationException("المستخدم غير موجود");
            user.IsActive = false;
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.LockReason = reason;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(userId, "Users", id, "DEACTIVATE", null, $"{{\"Reason\":\"{reason}\"}}");
        }

        public async Task LockUserAsync(int id, string reason, int userId)
        {
            if (id == userId) throw new InvalidOperationException("لا يمكنك قفل حسابك");
            var user = await _unitOfWork.GetByIdAsync<User>(id) ?? throw new InvalidOperationException("المستخدم غير موجود");
            user.IsLocked = true;
            user.LockReason = reason;
            user.LockedAt = DateTime.UtcNow;
            user.LockedById = userId;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(userId, "Users", id, "LOCK", null, $"{{\"Reason\":\"{reason}\"}}");
        }

        public async Task UnlockUserAsync(int id, int userId)
        {
            if (id == userId) throw new InvalidOperationException("لا يمكنك فك قفل حسابك بهذه العملية");
            var user = await _unitOfWork.GetByIdAsync<User>(id) ?? throw new InvalidOperationException("المستخدم غير موجود");
            user.IsLocked = false;
            user.LockReason = null;
            user.LockedAt = null;
            user.LockedById = null;
            user.FailedLoginCount = 0;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(userId, "Users", id, "UNLOCK", null, null);
        }

        public async Task AssignRoleAsync(int userId, int roleId, int assignedById)
        {
            await AssignUserRoleAsync(new AssignUserRoleDto { UserId = userId, RoleId = roleId }, assignedById);
        }

        public async Task AssignUserRoleAsync(AssignUserRoleDto dto, int assignedById)
        {
            if (dto.UserId == assignedById) throw new InvalidOperationException("لا يمكنك تعديل دور حسابك بهذه العملية");

            var user = await _unitOfWork.GetByIdAsync<User>(dto.UserId) ?? throw new InvalidOperationException("المستخدم غير موجود");
            var role = await _unitOfWork.GetQueryable<Role>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.RoleId && !x.IsDeleted)
                ?? throw new InvalidOperationException("الدور غير موجود");

            var selection = await NormalizeScopeSelectionAsync(
                role,
                dto.GovernorateId,
                dto.DistrictId,
                dto.SubDistrictId,
                dto.GovernorateIds,
                dto.DistrictIds,
                dto.SubDistrictIds,
                user);

            var oldRoleId = user.RoleId;
            user.RoleId = dto.RoleId;
            user.GovernorateId = selection.GovernorateId;
            user.DistrictId = selection.DistrictId;
            user.SubDistrictId = selection.SubDistrictId;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await SyncUserScopesAsync(user.Id, selection, assignedById);

            _permissionCacheService.InvalidateRole(oldRoleId);
            _permissionCacheService.InvalidateRole(dto.RoleId);
            await WriteAudit(assignedById, "Users", dto.UserId, "ASSIGN_ROLE", null, $"{{\"RoleId\":{dto.RoleId}}}");
        }

        public async Task<UserGeographicScopeSelectionDto> GetUserScopeSelectionAsync(int userId)
        {
            var user = await _unitOfWork.GetQueryable<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted)
                ?? throw new InvalidOperationException("المستخدم غير موجود");

            var assignments = await _unitOfWork.GetQueryable<UserGeographicScope>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.IsActive)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var primary = assignments.FirstOrDefault();

            return new UserGeographicScopeSelectionDto
            {
                GovernorateId = user.GovernorateId ?? primary?.GovernorateId,
                DistrictId = user.DistrictId ?? primary?.DistrictId,
                SubDistrictId = user.SubDistrictId ?? primary?.SubDistrictId,
                GovernorateIds = assignments.Where(x => x.GovernorateId.HasValue).Select(x => x.GovernorateId!.Value).Distinct().ToList(),
                DistrictIds = assignments.Where(x => x.DistrictId.HasValue).Select(x => x.DistrictId!.Value).Distinct().ToList(),
                SubDistrictIds = assignments.Where(x => x.SubDistrictId.HasValue).Select(x => x.SubDistrictId!.Value).Distinct().ToList()
            };
        }

        public async Task<string> ResetPasswordAsync(int userId, int adminId)
        {
            if (userId == adminId) throw new InvalidOperationException("لا يمكنك إعادة تعيين كلمة مرور حسابك بهذه العملية");
            var user = await _unitOfWork.GetByIdAsync<User>(userId) ?? throw new InvalidOperationException("المستخدم غير موجود");

            var tempPassword = GenerateTemporaryPassword(8);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.MustChangePassword = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await WriteAudit(adminId, "Users", userId, "RESET_PASSWORD", null, "{}");
            return tempPassword;
        }

        public async Task<List<GovernorateDto>> GetAllGovernoratesAsync(bool includeInactive = false)
        {
            var query = _unitOfWork.GetQueryable<Governorate>().AsNoTracking().Where(x => !x.IsDeleted);
            if (!includeInactive) query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr)
                .Select(x => new GovernorateDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Code = x.Code,
                    GisLayerId = x.GisLayerId,
                    PostalCode = x.PostalCode,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    DistrictCount = x.Districts.Count(d => !d.IsDeleted),
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<GovernorateDto?> GetGovernorateAsync(int id)
        {
            return await _unitOfWork.GetQueryable<Governorate>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new GovernorateDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Code = x.Code,
                    GisLayerId = x.GisLayerId,
                    PostalCode = x.PostalCode,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    DistrictCount = x.Districts.Count(d => !d.IsDeleted),
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateGovernorateAsync(CreateGovernorateDto dto, int userId)
        {
            var exists = await _unitOfWork.GetQueryable<Governorate>().AsNoTracking().AnyAsync(x => x.Code == dto.Code && !x.IsDeleted);
            if (exists) throw new InvalidOperationException("كود المحافظة مستخدم مسبقًا");

            var entity = new Governorate
            {
                CountryId = 1,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Code = dto.Code,
                GisLayerId = dto.GisLayerId,
                PostalCode = dto.PostalCode,
                SortOrder = dto.SortOrder,
                IsActive = true,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Governorates", entity.Id, "CREATE", null, $"{{\"NameAr\":\"{entity.NameAr}\"}}");
            return entity.Id;
        }

        public async Task UpdateGovernorateAsync(UpdateGovernorateDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Governorate>(dto.Id) ?? throw new InvalidOperationException("المحافظة غير موجودة");
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Code = dto.Code;
            entity.GisLayerId = dto.GisLayerId;
            entity.PostalCode = dto.PostalCode;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Governorates", entity.Id, "UPDATE", null, null);
        }

        public async Task ToggleGovernorateAsync(int id, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Governorate>(id) ?? throw new InvalidOperationException("المحافظة غير موجودة");

            if (entity.IsActive)
            {
                var hasActiveProperties = await _unitOfWork.GetQueryable<Property>().AsNoTracking().AnyAsync(x => !x.IsDeleted && x.GovernorateId == id && x.PropertyStatus == Core.Enums.PropertyStatus.Active);
                if (hasActiveProperties) throw new InvalidOperationException("لا يمكن إيقاف المحافظة لوجود عقارات نشطة مرتبطة بها");
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Governorates", entity.Id, "TOGGLE", null, $"{{\"IsActive\":{entity.IsActive.ToString().ToLowerInvariant()}}}");
        }

        public async Task<List<DistrictDto>> GetDistrictsAsync(int governorateId, bool includeInactive = false)
        {
            var query = _unitOfWork.GetQueryable<District>().AsNoTracking().Where(x => !x.IsDeleted && x.GovernorateId == governorateId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).Select(x => new DistrictDto
            {
                Id = x.Id,
                GovernorateId = x.GovernorateId,
                GovernorateNameAr = x.Governorate.NameAr,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Code = x.Code,
                GisLayerId = x.GisLayerId,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder,
                SubDistrictCount = x.SubDistricts.Count(sd => !sd.IsDeleted)
            }).ToListAsync();
        }

        public async Task<int> CreateDistrictAsync(CreateDistrictDto dto, int userId)
        {
            var entity = new District
            {
                GovernorateId = dto.GovernorateId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Code = dto.Code ?? string.Empty,
                GisLayerId = dto.GisLayerId,
                SortOrder = dto.SortOrder,
                IsActive = true,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Districts", entity.Id, "CREATE", null, null);
            return entity.Id;
        }

        public async Task UpdateDistrictAsync(UpdateDistrictDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<District>(dto.Id) ?? throw new InvalidOperationException("القضاء غير موجود");
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Code = dto.Code ?? entity.Code;
            entity.GisLayerId = dto.GisLayerId;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Districts", entity.Id, "UPDATE", null, null);
        }

        public async Task ToggleDistrictAsync(int id, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<District>(id) ?? throw new InvalidOperationException("القضاء غير موجود");
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Districts", entity.Id, "TOGGLE", null, null);
        }

        public async Task<List<SubDistrictDto>> GetSubDistrictsAsync(int districtId, bool includeInactive = false)
        {
            var query = _unitOfWork.GetQueryable<SubDistrict>().AsNoTracking().Where(x => !x.IsDeleted && x.DistrictId == districtId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).Select(x => new SubDistrictDto
            {
                Id = x.Id,
                DistrictId = x.DistrictId,
                DistrictNameAr = x.District.NameAr,
                GovernorateNameAr = x.District.Governorate.NameAr,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Code = x.Code,
                GisLayerId = x.GisLayerId,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder,
                NeighborhoodCount = x.Neighborhoods.Count(n => !n.IsDeleted)
            }).ToListAsync();
        }

        public async Task<int> CreateSubDistrictAsync(CreateSubDistrictDto dto, int userId)
        {
            var entity = new SubDistrict
            {
                DistrictId = dto.DistrictId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Code = dto.Code ?? string.Empty,
                GisLayerId = dto.GisLayerId,
                SortOrder = dto.SortOrder,
                IsActive = true,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "SubDistricts", entity.Id, "CREATE", null, null);
            return entity.Id;
        }

        public async Task UpdateSubDistrictAsync(UpdateSubDistrictDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<SubDistrict>(dto.Id) ?? throw new InvalidOperationException("الناحية غير موجودة");
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Code = dto.Code ?? entity.Code;
            entity.GisLayerId = dto.GisLayerId;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "SubDistricts", entity.Id, "UPDATE", null, null);
        }

        public async Task ToggleSubDistrictAsync(int id, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<SubDistrict>(id) ?? throw new InvalidOperationException("الناحية غير موجودة");
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "SubDistricts", entity.Id, "TOGGLE", null, null);
        }

        public async Task<List<NeighborhoodDto>> GetNeighborhoodsAsync(int subDistrictId, bool includeInactive = false)
        {
            var query = _unitOfWork.GetQueryable<Neighborhood>().AsNoTracking().Where(x => !x.IsDeleted && x.SubDistrictId == subDistrictId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).Select(x => new NeighborhoodDto
            {
                Id = x.Id,
                SubDistrictId = x.SubDistrictId,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Code = x.Code,
                Type = x.Type,
                PostalCode = x.PostalCode,
                GisLayerId = x.GisLayerId,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder,
                StreetCount = x.Streets.Count(s => !s.IsDeleted)
            }).ToListAsync();
        }

        public async Task<int> CreateNeighborhoodAsync(CreateNeighborhoodDto dto, int userId)
        {
            var entity = new Neighborhood
            {
                SubDistrictId = dto.SubDistrictId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Code = dto.Code ?? string.Empty,
                Type = dto.Type,
                PostalCode = dto.PostalCode,
                GisLayerId = dto.GisLayerId,
                SortOrder = dto.SortOrder,
                IsActive = true,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Neighborhoods", entity.Id, "CREATE", null, null);
            return entity.Id;
        }

        public async Task UpdateNeighborhoodAsync(UpdateNeighborhoodDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Neighborhood>(dto.Id) ?? throw new InvalidOperationException("الحي غير موجود");
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Code = dto.Code ?? entity.Code;
            entity.Type = dto.Type;
            entity.PostalCode = dto.PostalCode;
            entity.GisLayerId = dto.GisLayerId;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Neighborhoods", entity.Id, "UPDATE", null, null);
        }

        public async Task ToggleNeighborhoodAsync(int id, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Neighborhood>(id) ?? throw new InvalidOperationException("الحي غير موجود");
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Neighborhoods", entity.Id, "TOGGLE", null, null);
        }

        public async Task<List<StreetDto>> GetStreetsAsync(int neighborhoodId, bool includeInactive = false)
        {
            var query = _unitOfWork.GetQueryable<Street>().AsNoTracking().Where(x => !x.IsDeleted && x.NeighborhoodId == neighborhoodId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).Select(x => new StreetDto
            {
                Id = x.Id,
                NeighborhoodId = x.NeighborhoodId,
                NeighborhoodNameAr = x.Neighborhood.NameAr,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Code = x.Code,
                GisLineId = x.GisLineId,
                StreetType = x.StreetType,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToListAsync();
        }

        public async Task<int> CreateStreetAsync(CreateStreetDto dto, int userId)
        {
            var entity = new Street
            {
                NeighborhoodId = dto.NeighborhoodId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Code = dto.Code ?? string.Empty,
                GisLineId = dto.GisLineId,
                StreetType = dto.StreetType,
                SortOrder = dto.SortOrder,
                IsActive = true,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Streets", entity.Id, "CREATE", null, null);
            return entity.Id;
        }

        public async Task UpdateStreetAsync(UpdateStreetDto dto, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Street>(dto.Id) ?? throw new InvalidOperationException("الشارع غير موجود");
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Code = dto.Code ?? entity.Code;
            entity.GisLineId = dto.GisLineId;
            entity.StreetType = dto.StreetType;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Streets", entity.Id, "UPDATE", null, null);
        }

        public async Task ToggleStreetAsync(int id, int userId)
        {
            var entity = await _unitOfWork.GetByIdAsync<Street>(id) ?? throw new InvalidOperationException("الشارع غير موجود");
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            _geographicService.InvalidateGeographicCache();
            await WriteAudit(userId, "Streets", entity.Id, "TOGGLE", null, null);
        }

        public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterRequest filter)
        {
            IQueryable<AuditLog> query = _unitOfWork.GetQueryable<AuditLog>().AsNoTracking().Include(x => x.User);

            if (filter.UserId.HasValue) query = query.Where(x => x.UserId == filter.UserId);
            if (!string.IsNullOrWhiteSpace(filter.EntityType)) query = query.Where(x => x.TableName == filter.EntityType);
            if (!string.IsNullOrWhiteSpace(filter.Action)) query = query.Where(x => x.Action == filter.Action);
            if (filter.DateFrom.HasValue) query = query.Where(x => x.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(x => x.CreatedAt <= filter.DateTo.Value);
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(x => (x.NewValues ?? string.Empty).Contains(term) || (x.OldValues ?? string.Empty).Contains(term));
            }

            var total = await query.CountAsync();
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var size = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var logs = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new AuditLogDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User != null ? x.User.FullNameAr : "-",
                    Action = x.Action,
                    EntityType = x.TableName,
                    EntityId = x.RecordId,
                    OldValues = x.OldValues,
                    NewValues = x.NewValues,
                    IpAddress = x.IpAddress,
                    CreatedAt = x.CreatedAt
                }).ToListAsync();

            return new PagedResult<AuditLogDto>
            {
                Items = logs,
                TotalCount = total,
                Page = page,
                PageSize = size
            };
        }

        private async Task WriteAudit(int userId, string tableName, int recordId, string action, string? oldValues, string? newValues)
        {
            await _unitOfWork.AddAsync(new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<ScopeSelectionResult> NormalizeScopeSelectionAsync(
            Role role,
            int? governorateId,
            int? districtId,
            int? subDistrictId,
            List<int>? governorateIds,
            List<int>? districtIds,
            List<int>? subDistrictIds,
            User? existingUser = null)
        {
            if (role.HasGlobalScope || role.GeographicScopeLevel == GeographicScopeLevel.None)
            {
                return new ScopeSelectionResult { ScopeLevel = GeographicScopeLevel.None };
            }

            var result = new ScopeSelectionResult
            {
                ScopeLevel = role.GeographicScopeLevel,
                GovernorateIds = NormalizeIds(governorateIds, governorateId, existingUser?.GovernorateId),
                DistrictIds = NormalizeIds(districtIds, districtId, existingUser?.DistrictId),
                SubDistrictIds = NormalizeIds(subDistrictIds, subDistrictId, existingUser?.SubDistrictId)
            };

            if (role.GeographicScopeLevel == GeographicScopeLevel.Governorate)
            {
                if (result.GovernorateIds.Count == 0)
                {
                    throw new InvalidOperationException("يجب اختيار محافظة واحدة على الأقل لهذا الدور");
                }

                var validGovs = await _unitOfWork.GetQueryable<Governorate>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && result.GovernorateIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                if (validGovs.Count != result.GovernorateIds.Count)
                {
                    throw new InvalidOperationException("توجد محافظات غير صالحة في التحديد");
                }

                result.GovernorateId = PickPrimary(result.GovernorateIds, governorateId, existingUser?.GovernorateId);
                result.GovernorateIds = validGovs.Distinct().ToList();
                return result;
            }

            if (role.GeographicScopeLevel == GeographicScopeLevel.District)
            {
                if (result.DistrictIds.Count == 0)
                {
                    throw new InvalidOperationException("يجب اختيار قضاء واحد على الأقل لهذا الدور");
                }

                var districts = await _unitOfWork.GetQueryable<District>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && result.DistrictIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.GovernorateId })
                    .ToListAsync();

                if (districts.Count != result.DistrictIds.Count)
                {
                    throw new InvalidOperationException("توجد أقضية غير صالحة في التحديد");
                }

                result.DistrictIds = districts.Select(x => x.Id).Distinct().ToList();
                result.DistrictId = PickPrimary(result.DistrictIds, districtId, existingUser?.DistrictId);

                var districtGovs = districts.Select(x => x.GovernorateId).Distinct().ToList();
                result.GovernorateIds = districtGovs;
                result.GovernorateId = PickPrimary(result.GovernorateIds, governorateId, existingUser?.GovernorateId) ?? districtGovs.First();
                return result;
            }

            if (result.SubDistrictIds.Count == 0)
            {
                throw new InvalidOperationException("يجب اختيار ناحية واحدة على الأقل لهذا الدور");
            }

            var subDistricts = await _unitOfWork.GetQueryable<SubDistrict>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && result.SubDistrictIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DistrictId, GovernorateId = x.District.GovernorateId })
                .ToListAsync();

            if (subDistricts.Count != result.SubDistrictIds.Count)
            {
                throw new InvalidOperationException("توجد نواحٍ غير صالحة في التحديد");
            }

            result.SubDistrictIds = subDistricts.Select(x => x.Id).Distinct().ToList();
            result.SubDistrictId = PickPrimary(result.SubDistrictIds, subDistrictId, existingUser?.SubDistrictId);

            result.DistrictIds = subDistricts.Select(x => x.DistrictId).Distinct().ToList();
            result.DistrictId = PickPrimary(result.DistrictIds, districtId, existingUser?.DistrictId) ?? subDistricts.First().DistrictId;

            result.GovernorateIds = subDistricts.Select(x => x.GovernorateId).Distinct().ToList();
            result.GovernorateId = PickPrimary(result.GovernorateIds, governorateId, existingUser?.GovernorateId) ?? subDistricts.First().GovernorateId;

            return result;
        }

        private async Task SyncUserScopesAsync(int userId, ScopeSelectionResult selection, int createdById)
        {
            var existing = await _unitOfWork.GetQueryable<UserGeographicScope>()
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var item in existing)
            {
                await _unitOfWork.DeleteAsync(item);
            }

            if (selection.ScopeLevel == GeographicScopeLevel.None)
            {
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            if (selection.ScopeLevel == GeographicScopeLevel.Governorate)
            {
                foreach (var id in selection.GovernorateIds.Distinct())
                {
                    await _unitOfWork.AddAsync(new UserGeographicScope
                    {
                        UserId = userId,
                        ScopeLevel = GeographicScopeLevel.Governorate,
                        GovernorateId = id,
                        DistrictId = null,
                        SubDistrictId = null,
                        IsPrimary = selection.GovernorateId == id,
                        IsActive = true,
                        CreatedById = createdById,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return;
            }

            if (selection.ScopeLevel == GeographicScopeLevel.District)
            {
                var districts = await _unitOfWork.GetQueryable<District>()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && selection.DistrictIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.GovernorateId })
                    .ToListAsync();

                foreach (var district in districts)
                {
                    await _unitOfWork.AddAsync(new UserGeographicScope
                    {
                        UserId = userId,
                        ScopeLevel = GeographicScopeLevel.District,
                        GovernorateId = district.GovernorateId,
                        DistrictId = district.Id,
                        SubDistrictId = null,
                        IsPrimary = selection.DistrictId == district.Id,
                        IsActive = true,
                        CreatedById = createdById,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var subDistricts = await _unitOfWork.GetQueryable<SubDistrict>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted && selection.SubDistrictIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DistrictId, GovernorateId = x.District.GovernorateId })
                .ToListAsync();

            foreach (var subDistrict in subDistricts)
            {
                await _unitOfWork.AddAsync(new UserGeographicScope
                {
                    UserId = userId,
                    ScopeLevel = GeographicScopeLevel.SubDistrict,
                    GovernorateId = subDistrict.GovernorateId,
                    DistrictId = subDistrict.DistrictId,
                    SubDistrictId = subDistrict.Id,
                    IsPrimary = selection.SubDistrictId == subDistrict.Id,
                    IsActive = true,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private static List<int> NormalizeIds(List<int>? ids, int? primary, int? fallback)
        {
            var normalized = (ids ?? new List<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (primary.HasValue && primary.Value > 0 && !normalized.Contains(primary.Value))
            {
                normalized.Insert(0, primary.Value);
            }

            if (fallback.HasValue && fallback.Value > 0 && !normalized.Contains(fallback.Value))
            {
                normalized.Insert(0, fallback.Value);
            }

            return normalized.Distinct().ToList();
        }

        private static int? PickPrimary(List<int> ids, int? requestedPrimary, int? fallbackPrimary)
        {
            if (requestedPrimary.HasValue && ids.Contains(requestedPrimary.Value))
            {
                return requestedPrimary.Value;
            }

            if (fallbackPrimary.HasValue && ids.Contains(fallbackPrimary.Value))
            {
                return fallbackPrimary.Value;
            }

            return ids.FirstOrDefault();
        }

        private sealed class ScopeSelectionResult
        {
            public GeographicScopeLevel ScopeLevel { get; set; }
            public int? GovernorateId { get; set; }
            public int? DistrictId { get; set; }
            public int? SubDistrictId { get; set; }
            public List<int> GovernorateIds { get; set; } = new();
            public List<int> DistrictIds { get; set; } = new();
            public List<int> SubDistrictIds { get; set; } = new();
        }

        private static string GenerateTemporaryPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(chars[bytes[i] % chars.Length]);
            }

            return sb.ToString();
        }
    }
}
