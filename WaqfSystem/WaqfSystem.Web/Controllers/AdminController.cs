using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Admin;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Application.Services;
using WaqfSystem.Infrastructure.Authorization;
using WaqfSystem.Infrastructure.Data;
using WaqfSystem.Web.ViewModels.Admin;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IPermissionCacheService _permissionCacheService;
        private readonly WaqfDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, IPermissionCacheService permissionCacheService, WaqfDbContext db, IWebHostEnvironment environment, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _permissionCacheService = permissionCacheService;
            _db = db;
            _environment = environment;
            _logger = logger;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        [RequirePermission(PermissionKeys.Admin_ManageUsers)]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var dto = await _adminService.GetDashboardAsync();
            var vm = new AdminDashboardViewModel
            {
                Dashboard = dto,
                RecentActivity = dto.RecentAuditLogs
            };
            return View("Dashboard", vm);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _adminService.GetAllRolesAsync();
            var vm = new RolesViewModel
            {
                Roles = roles,
                TotalSystemRoles = roles.Count(x => x.IsSystemRole),
                TotalCustomRoles = roles.Count(x => !x.IsSystemRole)
            };
            return View("Roles", vm);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public IActionResult CreateRole()
        {
            var vm = BuildCreateRoleViewModel();
            return View("CreateEditRole", vm);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateEditRole", BuildCreateRoleViewModel(dto));
            }

            var id = await _adminService.CreateRoleAsync(dto, GetUserId());
            TempData["SuccessMessage"] = "تم إنشاء الدور بنجاح";
            return RedirectToAction(nameof(EditRole), new { id });
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public async Task<IActionResult> EditRole(int id)
        {
            var role = await _adminService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();

            var all = await _adminService.GetAllPermissionsGroupedAsync();
            var vm = BuildCreateRoleViewModel(new CreateRoleDto
            {
                Name = role.Name,
                DisplayNameAr = role.DisplayNameAr ?? string.Empty,
                DisplayNameEn = role.DisplayNameEn,
                Description = role.Description,
                Color = role.Color,
                Icon = role.Icon,
                GeographicScopeLevel = role.GeographicScopeLevel,
                HasGlobalScope = role.HasGlobalScope
            });
            vm.IsEditMode = true;
            vm.RoleId = role.Id;
            vm.PermissionGroups = all;
            vm.SelectedPermissionIds = all.SelectMany(x => x.Permissions)
                .Where(x => role.Permissions.Contains(x.PermissionKey))
                .Select(x => x.Id)
                .ToHashSet();

            return View("CreateEditRole", vm);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(UpdateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = BuildCreateRoleViewModel(new CreateRoleDto
                {
                    Name = dto.Name,
                    DisplayNameAr = dto.DisplayNameAr,
                    DisplayNameEn = dto.DisplayNameEn,
                    Description = dto.Description,
                    Color = dto.Color,
                    Icon = dto.Icon
                });
                vm.IsEditMode = true;
                vm.RoleId = dto.Id;
                return View("CreateEditRole", vm);
            }

            await _adminService.UpdateRoleAsync(dto, GetUserId());
            TempData["SuccessMessage"] = "تم تحديث الدور بنجاح";
            return RedirectToAction(nameof(Roles));
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                await _adminService.DeleteRoleAsync(id, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم حذف الدور", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        public async Task<IActionResult> SetPermissions(SetRolePermissionsDto dto)
        {
            try
            {
                await _adminService.SetRolePermissionsAsync(dto.RoleId, dto.PermissionIds, GetUserId());
                return Json(new { success = true, data = new { permissionCount = dto.PermissionIds.Count }, message = "تم تحديث الصلاحيات", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageUsers)]
        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] UserFilterRequest filter)
        {
            var users = await _adminService.GetUsersAsync(filter);
            var roles = await _adminService.GetAllRolesAsync();
            var govs = await _adminService.GetAllGovernoratesAsync(true);

            var vm = new UsersViewModel
            {
                Users = users,
                Filter = filter,
                Roles = roles.Select(x => new SelectListItem(x.DisplayNameAr, x.Id.ToString())).ToList(),
                Governorates = govs.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                RoleDetails = roles
            };
            return View("Users", vm);
        }

        [RequirePermission(PermissionKeys.Users_Create)]
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var vm = await BuildUserEditorViewModelAsync(new CreateUserDto(), false, 0);
            return View("CreateEditUser", vm);
        }

        [RequirePermission(PermissionKeys.Users_Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserDto dto, IFormFile? profilePhoto)
        {
            if (profilePhoto != null)
            {
                dto.ProfilePhotoUrl = await SaveProfilePhotoAsync(profilePhoto);
            }

            if (!ModelState.IsValid)
            {
                var vmInvalid = await BuildUserEditorViewModelAsync(dto, false, 0);
                return View("CreateEditUser", vmInvalid);
            }

            await _adminService.CreateUserAsync(dto, GetUserId());
            TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح";
            return RedirectToAction(nameof(Users));
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _adminService.GetAllRolesAsync();
            var role = roles.FirstOrDefault(r => r.Name == user.RoleName);
            var scopes = await _adminService.GetUserScopeSelectionAsync(id);

            var dto = new CreateUserDto
            {
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Email = user.Email,
                RoleId = role?.Id ?? 0,
                Phone = user.Phone,
                Phone2 = user.Phone2,
                NationalId = user.NationalId,
                JobTitle = user.JobTitle,
                GovernorateId = scopes.GovernorateId,
                DistrictId = scopes.DistrictId,
                SubDistrictId = scopes.SubDistrictId,
                GovernorateIds = scopes.GovernorateIds,
                DistrictIds = scopes.DistrictIds,
                SubDistrictIds = scopes.SubDistrictIds,
                Notes = user.Notes,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                MustChangePassword = user.MustChangePassword
            };

            var vm = await BuildUserEditorViewModelAsync(dto, true, id);

            return View("CreateEditUser", vm);
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UpdateUserDto dto, IFormFile? profilePhoto)
        {
            if (profilePhoto != null)
            {
                dto.ProfilePhotoUrl = await SaveProfilePhotoAsync(profilePhoto);
            }

            if (!ModelState.IsValid)
            {
                var vmInvalid = await BuildUserEditorViewModelAsync(new CreateUserDto
                {
                    FullNameAr = dto.FullNameAr,
                    FullNameEn = dto.FullNameEn,
                    Email = dto.Email,
                    RoleId = dto.RoleId,
                    Phone = dto.Phone,
                    Phone2 = dto.Phone2,
                    NationalId = dto.NationalId,
                    JobTitle = dto.JobTitle,
                    GovernorateId = dto.GovernorateId,
                    DistrictId = dto.DistrictId,
                    SubDistrictId = dto.SubDistrictId,
                    GovernorateIds = dto.GovernorateIds,
                    DistrictIds = dto.DistrictIds,
                    SubDistrictIds = dto.SubDistrictIds,
                    Notes = dto.Notes,
                    MustChangePassword = dto.MustChangePassword,
                    ProfilePhotoUrl = dto.ProfilePhotoUrl
                }, true, dto.Id);
                return View("CreateEditUser", vmInvalid);
            }

            await _adminService.UpdateUserAsync(dto, GetUserId());
            TempData["SuccessMessage"] = "تم تحديث المستخدم";
            return RedirectToAction(nameof(Users));
        }

        [RequirePermission(PermissionKeys.Users_View)]
        [HttpGet]
        public async Task<IActionResult> UserDetail(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var logs = await _adminService.GetAuditLogsAsync(new AuditLogFilterRequest { UserId = id, Page = 1, PageSize = 50 });
            var vm = new UserDetailViewModel
            {
                User = user,
                Roles = (await _adminService.GetAllRolesAsync()).Select(x => new SelectListItem(x.DisplayNameAr, x.Id.ToString())).ToList(),
                PermissionGroups = await _adminService.GetAllPermissionsGroupedAsync(),
                UserAuditLogs = logs
            };
            return View("UserDetail", vm);
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpPost]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                await _adminService.ActivateUserAsync(id, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم تفعيل المستخدم", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_Deactivate)]
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int id, string reason)
        {
            try
            {
                await _adminService.DeactivateUserAsync(id, reason, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم إيقاف المستخدم", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_Deactivate)]
        [HttpPost]
        public async Task<IActionResult> LockUser(int id, string reason)
        {
            try
            {
                await _adminService.LockUserAsync(id, reason, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم قفل الحساب", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpPost]
        public async Task<IActionResult> UnlockUser(int id)
        {
            try
            {
                await _adminService.UnlockUserAsync(id, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم فك القفل", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var password = await _adminService.ResetPasswordAsync(id, GetUserId());
                return Json(new { success = true, data = new { tempPassword = password }, message = "تمت إعادة التعيين", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_Edit)]
        [HttpPost]
        public async Task<IActionResult> AssignRole([FromBody] AssignUserRoleDto dto)
        {
            try
            {
                await _adminService.AssignUserRoleAsync(dto, GetUserId());
                var role = await _adminService.GetRoleByIdAsync(dto.RoleId);
                return Json(new { success = true, data = new { roleName = role?.DisplayNameAr ?? string.Empty }, message = "تم تغيير الدور", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Users_View)]
        [HttpGet]
        public async Task<IActionResult> GetUserScopeSelection(int userId)
        {
            try
            {
                var selection = await _adminService.GetUserScopeSelectionAsync(userId);
                return Json(new { success = true, data = selection, message = string.Empty, errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpGet]
        public async Task<IActionResult> Geographic()
        {
            var govs = await _adminService.GetAllGovernoratesAsync(true);
            var vm = new GeographicViewModel
            {
                Governorates = govs,
                TotalCounts = new GeographicCountsViewModel
                {
                    Districts = govs.Sum(x => x.DistrictCount),
                    SubDistricts = 0,
                    Neighborhoods = 0,
                    Streets = 0
                }
            };
            return View("Geographic", vm);
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> CreateGovernorate(CreateGovernorateDto dto)
        {
            try
            {
                var id = await _adminService.CreateGovernorateAsync(dto, GetUserId());
                var item = await _adminService.GetGovernorateAsync(id);
                return Json(new { success = true, data = new { id, item }, message = "تمت الإضافة", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> UpdateGovernorate(UpdateGovernorateDto dto)
        {
            try
            {
                await _adminService.UpdateGovernorateAsync(dto, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم التحديث", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> ToggleGovernorate(int id)
        {
            try
            {
                await _adminService.ToggleGovernorateAsync(id, GetUserId());
                var gov = await _adminService.GetGovernorateAsync(id);
                return Json(new { success = true, data = new { isActive = gov?.IsActive ?? false }, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> CreateDistrict(CreateDistrictDto dto)
        {
            try
            {
                var id = await _adminService.CreateDistrictAsync(dto, GetUserId());
                return Json(new { success = true, data = new { id }, message = "تمت الإضافة", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> UpdateDistrict(UpdateDistrictDto dto)
        {
            try
            {
                await _adminService.UpdateDistrictAsync(dto, GetUserId());
                return Json(new { success = true, data = (object?)null, message = "تم التحديث", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> ToggleDistrict(int id)
        {
            await _adminService.ToggleDistrictAsync(id, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> CreateSubDistrict(CreateSubDistrictDto dto)
        {
            var id = await _adminService.CreateSubDistrictAsync(dto, GetUserId());
            return Json(new { success = true, data = new { id }, message = "تمت الإضافة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> UpdateSubDistrict(UpdateSubDistrictDto dto)
        {
            await _adminService.UpdateSubDistrictAsync(dto, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم التحديث", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> ToggleSubDistrict(int id)
        {
            await _adminService.ToggleSubDistrictAsync(id, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> CreateNeighborhood(CreateNeighborhoodDto dto)
        {
            var id = await _adminService.CreateNeighborhoodAsync(dto, GetUserId());
            return Json(new { success = true, data = new { id }, message = "تمت الإضافة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> UpdateNeighborhood(UpdateNeighborhoodDto dto)
        {
            await _adminService.UpdateNeighborhoodAsync(dto, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم التحديث", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> ToggleNeighborhood(int id)
        {
            await _adminService.ToggleNeighborhoodAsync(id, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> CreateStreet(CreateStreetDto dto)
        {
            var id = await _adminService.CreateStreetAsync(dto, GetUserId());
            return Json(new { success = true, data = new { id }, message = "تمت الإضافة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> UpdateStreet(UpdateStreetDto dto)
        {
            await _adminService.UpdateStreetAsync(dto, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم التحديث", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpPost]
        public async Task<IActionResult> ToggleStreet(int id)
        {
            await _adminService.ToggleStreetAsync(id, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpGet]
        public async Task<IActionResult> GetDistricts(int governorateId)
        {
            var list = await _adminService.GetDistrictsAsync(governorateId, true);
            return Json(new { success = true, data = list, message = "", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpGet]
        public async Task<IActionResult> GetSubDistricts(int districtId)
        {
            var list = await _adminService.GetSubDistrictsAsync(districtId, true);
            return Json(new { success = true, data = list, message = "", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpGet]
        public async Task<IActionResult> GetNeighborhoods(int subDistrictId)
        {
            var list = await _adminService.GetNeighborhoodsAsync(subDistrictId, true);
            return Json(new { success = true, data = list, message = "", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ManageGeo)]
        [HttpGet]
        public async Task<IActionResult> GetStreets(int neighborhoodId)
        {
            var list = await _adminService.GetStreetsAsync(neighborhoodId, true);
            return Json(new { success = true, data = list, message = "", errors = Array.Empty<string>() });
        }

        [RequirePermission(PermissionKeys.Admin_ViewAuditLog)]
        [HttpGet]
        public async Task<IActionResult> AuditLog([FromQuery] AuditLogFilterRequest filter)
        {
            var logs = await _adminService.GetAuditLogsAsync(filter);
            var vm = new AuditLogViewModel
            {
                Logs = logs,
                Filter = filter,
                EntityTypes = logs.Items.Select(x => x.EntityType).Distinct().OrderBy(x => x).Select(x => new SelectListItem(x, x)).ToList()
            };
            return View("AuditLog", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied(string? permission)
        {
            ViewBag.Permission = permission;
            return View();
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public async Task<IActionResult> DocumentTypes()
        {
            var rows = await _db.DocumentTypes
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.NameAr)
                .Select(x => new DocumentTypeDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Category = x.Category,
                    IsRequired = x.IsRequired,
                    HasExpiry = x.HasExpiry,
                    AlertDays1 = x.AlertDays1,
                    AlertDays2 = x.AlertDays2,
                    AllowedExtensions = x.AllowedExtensions,
                    VerifierRoles = new System.Collections.Generic.List<string>(),
                    IsActive = x.IsActive,
                    DocumentCount = _db.PropertyDocuments.Count(d => d.DocumentTypeId == x.Id)
                })
                .ToListAsync();

            var list = rows.Select(x =>
            {
                var roles = new System.Collections.Generic.List<string>();
                var raw = _db.DocumentTypes.Where(d => d.Id == x.Id).Select(d => d.VerifierRoles).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    roles = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(raw) ?? new System.Collections.Generic.List<string>();
                }

                x.VerifierRoles = roles;
                return x;
            }).ToList();

            return View("DocumentTypes", list);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public IActionResult CreateDocumentType()
        {
            return View("DocumentTypeForm", new CreateDocumentTypeDto());
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDocumentType(CreateDocumentTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("DocumentTypeForm", dto);
            }

            var entity = new Core.Entities.DocumentType
            {
                Code = dto.Code,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Category = dto.Category,
                Description = dto.Description,
                IsRequired = dto.IsRequired,
                HasExpiry = dto.HasExpiry,
                AlertDays1 = dto.AlertDays1,
                AlertDays2 = dto.AlertDays2,
                AllowedExtensions = dto.AllowedExtensions,
                MaxFileSizeMB = dto.MaxFileSizeMB,
                VerifierRoles = System.Text.Json.JsonSerializer.Serialize(dto.VerifierRoles),
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedById = GetUserId()
            };

            _db.DocumentTypes.Add(entity);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إنشاء نوع الوثيقة";
            return RedirectToAction(nameof(DocumentTypes));
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpGet]
        public async Task<IActionResult> EditDocumentType(int id)
        {
            var e = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();

            var dto = new UpdateDocumentTypeDto
            {
                Id = e.Id,
                Code = e.Code,
                NameAr = e.NameAr,
                NameEn = e.NameEn,
                Category = e.Category,
                Description = e.Description,
                IsRequired = e.IsRequired,
                HasExpiry = e.HasExpiry,
                AlertDays1 = e.AlertDays1,
                AlertDays2 = e.AlertDays2,
                AllowedExtensions = e.AllowedExtensions,
                MaxFileSizeMB = e.MaxFileSizeMB,
                VerifierRoles = string.IsNullOrWhiteSpace(e.VerifierRoles) ? new System.Collections.Generic.List<string>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(e.VerifierRoles) ?? new System.Collections.Generic.List<string>(),
                IsActive = e.IsActive,
                SortOrder = e.SortOrder
            };

            return View("DocumentTypeForm", dto);
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocumentType(UpdateDocumentTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("DocumentTypeForm", dto);
            }

            var e = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (e == null) return NotFound();

            e.Code = dto.Code;
            e.NameAr = dto.NameAr;
            e.NameEn = dto.NameEn;
            e.Category = dto.Category;
            e.Description = dto.Description;
            e.IsRequired = dto.IsRequired;
            e.HasExpiry = dto.HasExpiry;
            e.AlertDays1 = dto.AlertDays1;
            e.AlertDays2 = dto.AlertDays2;
            e.AllowedExtensions = dto.AllowedExtensions;
            e.MaxFileSizeMB = dto.MaxFileSizeMB;
            e.VerifierRoles = System.Text.Json.JsonSerializer.Serialize(dto.VerifierRoles);
            e.IsActive = dto.IsActive;
            e.SortOrder = dto.SortOrder;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث نوع الوثيقة";
            return RedirectToAction(nameof(DocumentTypes));
        }

        [RequirePermission(PermissionKeys.Admin_ManageRoles)]
        [HttpPost]
        public async Task<IActionResult> ToggleDocumentType(int id)
        {
            var e = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null)
            {
                return Json(new { success = false, data = (object?)null, message = "النوع غير موجود", errors = new[] { "النوع غير موجود" } });
            }

            e.IsActive = !e.IsActive;
            await _db.SaveChangesAsync();
            return Json(new { success = true, data = new { isActive = e.IsActive }, message = "تم تغيير الحالة", errors = Array.Empty<string>() });
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailUnique(string email, int? id)
        {
            var users = await _adminService.GetUsersAsync(new UserFilterRequest { SearchTerm = email, Page = 1, PageSize = 1 });
            var exists = users.Items.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && (!id.HasValue || x.Id != id.Value));
            return Json(new { success = true, data = new { isUnique = !exists }, message = "", errors = Array.Empty<string>() });
        }

        private CreateEditRoleViewModel BuildCreateRoleViewModel(CreateRoleDto? dto = null)
        {
            return new CreateEditRoleViewModel
            {
                Role = dto ?? new CreateRoleDto(),
                AvailableColors = new[] { "#0D5C3A", "#DC2626", "#1E40AF", "#7C3AED", "#065F46", "#0E7490", "#6B21A8", "#1D4ED8", "#92400E", "#0F766E", "#B45309", "#374151" }.ToList(),
                AvailableIcons = new[] { "bi-shield", "bi-shield-fill-check", "bi-building", "bi-geo-alt-fill", "bi-file-earmark-text", "bi-tools", "bi-person-badge", "bi-clipboard-check", "bi-cash-coin", "bi-file-earmark-ruled", "bi-graph-up", "bi-people" }.ToList()
            };
        }

        private async Task<string> SaveProfilePhotoAsync(IFormFile photo)
        {
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "users");
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            await using var stream = System.IO.File.Create(fullPath);
            await photo.CopyToAsync(stream);
            return $"/uploads/users/{fileName}";
        }

        private async Task<CreateEditUserViewModel> BuildUserEditorViewModelAsync(CreateUserDto user, bool isEditMode, int userId)
        {
            var roles = await _adminService.GetAllRolesAsync();
            var governorates = await _adminService.GetAllGovernoratesAsync(true);

            var governorateIds = (user.GovernorateIds ?? new System.Collections.Generic.List<int>()).Where(x => x > 0).Distinct().ToList();
            if (user.GovernorateId.HasValue && user.GovernorateId.Value > 0 && !governorateIds.Contains(user.GovernorateId.Value))
            {
                governorateIds.Add(user.GovernorateId.Value);
            }

            var districtIds = (user.DistrictIds ?? new System.Collections.Generic.List<int>()).Where(x => x > 0).Distinct().ToList();
            if (user.DistrictId.HasValue && user.DistrictId.Value > 0 && !districtIds.Contains(user.DistrictId.Value))
            {
                districtIds.Add(user.DistrictId.Value);
            }

            var subDistrictIds = (user.SubDistrictIds ?? new System.Collections.Generic.List<int>()).Where(x => x > 0).Distinct().ToList();
            if (user.SubDistrictId.HasValue && user.SubDistrictId.Value > 0 && !subDistrictIds.Contains(user.SubDistrictId.Value))
            {
                subDistrictIds.Add(user.SubDistrictId.Value);
            }

            var districtLookup = new System.Collections.Generic.List<SelectListItem>();
            foreach (var governorateId in governorateIds)
            {
                var list = await _adminService.GetDistrictsAsync(governorateId, true);
                districtLookup.AddRange(list.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())));
            }

            var subDistrictLookup = new System.Collections.Generic.List<SelectListItem>();
            foreach (var districtId in districtIds)
            {
                var list = await _adminService.GetSubDistrictsAsync(districtId, true);
                subDistrictLookup.AddRange(list.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())));
            }

            return new CreateEditUserViewModel
            {
                User = user,
                IsEditMode = isEditMode,
                UserId = userId,
                Roles = roles.Select(x => new SelectListItem(x.DisplayNameAr, x.Id.ToString())).ToList(),
                Governorates = governorates.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                Districts = districtLookup.GroupBy(x => x.Value).Select(x => x.First()).ToList(),
                SubDistricts = subDistrictLookup.GroupBy(x => x.Value).Select(x => x.First()).ToList(),
                RoleScopeLevels = roles.ToDictionary(x => x.Id, x => (int)x.GeographicScopeLevel),
                GlobalScopeRoleIds = roles.Where(x => x.HasGlobalScope).Select(x => x.Id).ToHashSet(),
                SelectedGovernorateIds = governorateIds,
                SelectedDistrictIds = districtIds,
                SelectedSubDistrictIds = subDistrictIds
            };
        }
    }
}
