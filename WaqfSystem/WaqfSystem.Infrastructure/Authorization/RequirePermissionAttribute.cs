using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        public string PermissionKey { get; }

        public RequirePermissionAttribute(string permissionKey)
        {
            PermissionKey = permissionKey;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path.ToString() });
                return;
            }

            var roleName = context.HttpContext.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            if (string.Equals(roleName, "SYS_ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var roleIdText = context.HttpContext.User.FindFirst("RoleId")?.Value;
            var roleId = 0;
            if (!int.TryParse(roleIdText, out roleId))
            {
                var userIdText = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdText, out var userId))
                {
                    var uow = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                    var user = uow.GetQueryable<User>().Where(x => x.Id == userId).Select(x => new { x.RoleId }).FirstOrDefault();
                    roleId = user?.RoleId ?? 0;
                }
            }

            if (roleId <= 0)
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissionCache = context.HttpContext.RequestServices.GetRequiredService<IPermissionCacheService>();
            var rolePermissions = permissionCache.GetRolePermissionsAsync(roleId).GetAwaiter().GetResult();

            if (!rolePermissions.Contains(PermissionKey))
            {
                var factory = context.HttpContext.RequestServices.GetService<ITempDataDictionaryFactory>();
                var tempData = factory?.GetTempData(context.HttpContext);
                if (tempData != null)
                {
                    tempData["PermissionDenied"] = PermissionKey;
                }

                context.Result = new ForbidResult();
            }
        }
    }

    public static class PermissionKeys
    {
        public const string Properties_View = "Properties.View";
        public const string Properties_Create = "Properties.Create";
        public const string Properties_Edit = "Properties.Edit";
        public const string Properties_Delete = "Properties.Delete";
        public const string Properties_Approve = "Properties.Approve";

        public const string Missions_View = "Missions.View";
        public const string Missions_Create = "Missions.Create";
        public const string Missions_Assign = "Missions.Assign";
        public const string Missions_Approve = "Missions.Approve";
        public const string Missions_Cancel = "Missions.Cancel";

        public const string Partnership_View = "Partnership.View";
        public const string Partnership_Create = "Partnership.Create";
        public const string Partnership_Revenue = "Partnership.Revenue";

        public const string Users_View = "Users.View";
        public const string Users_Create = "Users.Create";
        public const string Users_Edit = "Users.Edit";
        public const string Users_Deactivate = "Users.Deactivate";

        public const string Admin_ManageRoles = "Admin.ManageRoles";
        public const string Admin_ManageUsers = "Admin.ManageUsers";
        public const string Admin_ManageGeo = "Admin.ManageGeo";
        public const string Admin_ViewAuditLog = "Admin.ViewAuditLog";

        public const string Documents_View = "Documents.View";
        public const string Documents_Upload = "Documents.Upload";
        public const string Documents_Verify = "Documents.Verify";

        public const string Reports_View = "Reports.View";
        public const string Reports_Export = "Reports.Export";

        public const string Dashboard_View = "Dashboard.View";
        public const string Notifications_View = "Notifications.View";
        public const string Settings_Manage = "Settings.Manage";
    }
}
