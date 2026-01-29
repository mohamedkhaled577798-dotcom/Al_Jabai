using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Enums;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PermissionService _permissionService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork,
        PermissionService permissionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var users = await _userManager.Users
            .Include(u => u.WaqfOffice)
            .Include(u => u.Province)
            .ToListAsync();
        
        if (!string.IsNullOrEmpty(search))
            users = users.Where(u => u.FullNameAr.Contains(search) || 
                                      u.UserName!.Contains(search) ||
                                      u.Email!.Contains(search)).ToList();

        var userViewModels = new List<UserListViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                FullNameAr = user.FullNameAr,
                Email = user.Email!,
                OfficeName = user.WaqfOffice?.NameAr,
                ProvinceName = user.Province?.NameAr,
                PermissionLevel = user.PermissionLevel,
                PermissionLevelText = PermissionService.GetPermissionLevelDescription(user.PermissionLevel),
                Roles = string.Join(", ", roles),
                IsActive = user.IsActive,
                LastLogin = user.LastLogin
            });
        }

        ViewBag.CurrentSearch = search;
        return View(userViewModels);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewDataAsync();
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FullNameAr = model.FullNameAr,
            FullNameEn = model.FullNameEn,
            Phone = model.Phone,
            JobTitle = model.JobTitle,
            WaqfOfficeId = model.WaqfOfficeId,
            ProvinceId = model.ProvinceId,
            PermissionLevel = model.PermissionLevel,
            IsActive = true,
            MustChangePassword = true,
            EmailConfirmed = true,
            CreatedBy = User.Identity?.Name
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "تم إنشاء المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        await LoadViewDataAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.WaqfOffice)
            .Include(u => u.Province)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullNameAr = user.FullNameAr,
            FullNameEn = user.FullNameEn,
            Phone = user.Phone,
            JobTitle = user.JobTitle,
            WaqfOfficeId = user.WaqfOfficeId,
            ProvinceId = user.ProvinceId,
            PermissionLevel = user.PermissionLevel,
            Role = roles.FirstOrDefault(),
            IsActive = user.IsActive
        };

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.Email = model.Email;
        user.FullNameAr = model.FullNameAr;
        user.FullNameEn = model.FullNameEn;
        user.Phone = model.Phone;
        user.JobTitle = model.JobTitle;
        user.WaqfOfficeId = model.WaqfOfficeId;
        user.ProvinceId = model.ProvinceId;
        user.PermissionLevel = model.PermissionLevel;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "تم تحديث بيانات المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, "NewPass@123");

        if (result.Succeeded)
        {
            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "تم إعادة تعيين كلمة المرور إلى: NewPass@123";
        }
        else
            TempData["Error"] = "فشل في إعادة تعيين كلمة المرور";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.UserName == User.Identity?.Name)
        {
            TempData["Error"] = "لا يمكنك حذف حسابك الخاص";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "تم حذف المستخدم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadViewDataAsync()
    {
        ViewBag.WaqfOffices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
        
        // مستويات الصلاحيات
        ViewBag.PermissionLevels = Enum.GetValues<PermissionLevel>()
            .Select(p => new SelectListItem 
            { 
                Value = ((int)p).ToString(), 
                Text = PermissionService.GetPermissionLevelDescription(p) 
            }).ToList();
    }
}
