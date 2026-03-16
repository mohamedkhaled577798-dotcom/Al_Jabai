using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class PermissionDiscoveryService : IPermissionDiscoveryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionDiscoveryService> _logger;

        public PermissionDiscoveryService(IServiceProvider serviceProvider, IUnitOfWork unitOfWork, ILogger<PermissionDiscoveryService> logger)
        {
            _serviceProvider = serviceProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SyncPermissionsAsync()
        {
            var discovered = DiscoverPermissions();
            var keys = discovered.OrderBy(x => x).ToList();

            var existing = await _unitOfWork.GetQueryable<Permission>()
                .ToDictionaryAsync(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase);

            var newCount = 0;
            var existingCount = 0;
            foreach (var permissionKey in keys)
            {
                var meta = PermissionMeta.TryGetValue(permissionKey, out var value)
                    ? value
                    : (NameAr: permissionKey, NameEn: permissionKey, Module: permissionKey.Split('.')[0]);

                var action = permissionKey.Contains('.') ? permissionKey.Split('.')[1] : permissionKey;

                if (!existing.TryGetValue(permissionKey, out var entity))
                {
                    var permission = new Permission
                    {
                        PermissionKey = permissionKey,
                        Module = meta.Module,
                        Action = action,
                        DisplayNameAr = meta.NameAr,
                        DisplayNameEn = meta.NameEn,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.AddAsync(permission);
                    newCount++;
                }
                else
                {
                    entity.Module = meta.Module;
                    entity.Action = action;
                    entity.DisplayNameAr = meta.NameAr;
                    entity.DisplayNameEn = meta.NameEn;
                    await _unitOfWork.UpdateAsync(entity);
                    existingCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Discovered {Count} permissions, {NewCount} new, {ExistingCount} existing", keys.Count, newCount, existingCount);
        }

        private static HashSet<string> DiscoverPermissions()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in types)
                {
                    var classAttrs = type.GetCustomAttributes(true)
                        .Where(a => string.Equals(a.GetType().Name, "RequirePermissionAttribute", StringComparison.Ordinal));
                    foreach (var attr in classAttrs)
                    {
                        var key = attr.GetType().GetProperty("PermissionKey")?.GetValue(attr)?.ToString();
                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            result.Add(key);
                        }
                    }

                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var method in methods)
                    {
                        var methodAttrs = method.GetCustomAttributes(true)
                            .Where(a => string.Equals(a.GetType().Name, "RequirePermissionAttribute", StringComparison.Ordinal));
                        foreach (var attr in methodAttrs)
                        {
                            var key = attr.GetType().GetProperty("PermissionKey")?.GetValue(attr)?.ToString();
                            if (!string.IsNullOrWhiteSpace(key))
                            {
                                result.Add(key);
                            }
                        }
                    }
                }
            }

            foreach (var key in PermissionMeta.Keys)
            {
                result.Add(key);
            }

            return result;
        }

        public static readonly Dictionary<string, (string NameAr, string NameEn, string Module)> PermissionMeta = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Properties.View"] = ("عرض العقارات", "View Properties", "Properties"),
            ["Properties.Create"] = ("إنشاء عقار", "Create Property", "Properties"),
            ["Properties.Edit"] = ("تعديل عقار", "Edit Property", "Properties"),
            ["Properties.Delete"] = ("حذف عقار", "Delete Property", "Properties"),
            ["Properties.Approve"] = ("اعتماد عقار", "Approve Property", "Properties"),
            ["Missions.View"] = ("عرض المهام", "View Missions", "Missions"),
            ["Missions.Create"] = ("إنشاء مهمة", "Create Mission", "Missions"),
            ["Missions.Assign"] = ("إسناد مهمة", "Assign Mission", "Missions"),
            ["Missions.Approve"] = ("اعتماد مهمة", "Approve Mission", "Missions"),
            ["Missions.Cancel"] = ("إلغاء مهمة", "Cancel Mission", "Missions"),
            ["Partnership.View"] = ("عرض الشراكات", "View Partnership", "Partnership"),
            ["Partnership.Create"] = ("إنشاء شراكة", "Create Partnership", "Partnership"),
            ["Partnership.Revenue"] = ("إدارة إيرادات الشراكة", "Manage Partnership Revenue", "Partnership"),
            ["Users.View"] = ("عرض المستخدمين", "View Users", "Users"),
            ["Users.Create"] = ("إنشاء مستخدم", "Create User", "Users"),
            ["Users.Edit"] = ("تعديل مستخدم", "Edit User", "Users"),
            ["Users.Deactivate"] = ("إيقاف مستخدم", "Deactivate User", "Users"),
            ["Admin.ManageRoles"] = ("إدارة الأدوار", "Manage Roles", "Admin"),
            ["Admin.ManageUsers"] = ("إدارة المستخدمين", "Manage Users", "Admin"),
            ["Admin.ManageGeo"] = ("إدارة التسلسل الجغرافي", "Manage Geographic", "Admin"),
            ["Admin.ViewAuditLog"] = ("عرض سجل التدقيق", "View Audit Log", "Admin"),
            ["Documents.View"] = ("عرض الوثائق", "View Documents", "Documents"),
            ["Documents.Upload"] = ("رفع الوثائق", "Upload Documents", "Documents"),
            ["Documents.Verify"] = ("توثيق الوثائق", "Verify Documents", "Documents"),
            ["Reports.View"] = ("عرض التقارير", "View Reports", "Reports"),
            ["Reports.Export"] = ("تصدير التقارير", "Export Reports", "Reports"),
            ["Dashboard.View"] = ("عرض لوحة المتابعة", "View Dashboard", "Dashboard"),
            ["Notifications.View"] = ("عرض التنبيهات", "View Notifications", "Notifications"),
            ["Settings.Manage"] = ("إدارة الإعدادات", "Manage Settings", "Settings")
        };
    }
}
