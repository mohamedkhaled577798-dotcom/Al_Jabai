using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Infrastructure.Data;

/// <summary>
/// زرع البيانات الأولية
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);

        // Seed Lookup Data
        await SeedLookupDataAsync(context);

        // Seed Provinces
        await SeedProvincesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "Admin", "Editor", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@waqf.gov.iq";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullNameAr = "مدير النظام",
                FullNameEn = "System Administrator",
                EmailConfirmed = true,
                IsActive = true,
                MustChangePassword = false
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }

    private static async Task SeedLookupDataAsync(ApplicationDbContext context)
    {
        // Office Types
        if (!await context.OfficeTypes.AnyAsync())
        {
            context.OfficeTypes.AddRange(
                new OfficeType { NameAr = "ديوان الوقف", NameEn = "Waqf Diwan", Level = 1 },
                new OfficeType { NameAr = "مديرية الوقف", NameEn = "Waqf Directorate", Level = 2 },
                new OfficeType { NameAr = "دائرة الوقف", NameEn = "Waqf Department", Level = 3 },
                new OfficeType { NameAr = "شعبة الوقف", NameEn = "Waqf Division", Level = 4 }
            );
            await context.SaveChangesAsync();
        }

        // Mosque Types
        if (!await context.MosqueTypes.AnyAsync())
        {
            context.MosqueTypes.AddRange(
                new MosqueType { NameAr = "جامع", NameEn = "Grand Mosque", IconName = "mosque-grand" },
                new MosqueType { NameAr = "مسجد", NameEn = "Mosque", IconName = "mosque" },
                new MosqueType { NameAr = "مصلى", NameEn = "Prayer Room", IconName = "prayer-room" }
            );
            await context.SaveChangesAsync();
        }

        // Mosque Statuses
        if (!await context.MosqueStatuses.AnyAsync())
        {
            context.MosqueStatuses.AddRange(
                new MosqueStatus { NameAr = "فعال", NameEn = "Active", ColorCode = "#28a745" },
                new MosqueStatus { NameAr = "مغلق مؤقتاً", NameEn = "Temporarily Closed", ColorCode = "#ffc107" },
                new MosqueStatus { NameAr = "مغلق", NameEn = "Closed", ColorCode = "#dc3545" },
                new MosqueStatus { NameAr = "قيد الإنشاء", NameEn = "Under Construction", ColorCode = "#17a2b8" },
                new MosqueStatus { NameAr = "قيد الصيانة", NameEn = "Under Maintenance", ColorCode = "#6c757d" }
            );
            await context.SaveChangesAsync();
        }

        // Property Types
        if (!await context.PropertyTypes.AnyAsync())
        {
            context.PropertyTypes.AddRange(
                new PropertyType { NameAr = "أرض", NameEn = "Land", IconName = "land" },
                new PropertyType { NameAr = "مبنى", NameEn = "Building", IconName = "building" },
                new PropertyType { NameAr = "شقة", NameEn = "Apartment", IconName = "apartment" },
                new PropertyType { NameAr = "محل تجاري", NameEn = "Commercial Shop", IconName = "shop" },
                new PropertyType { NameAr = "مخزن", NameEn = "Warehouse", IconName = "warehouse" },
                new PropertyType { NameAr = "مزرعة", NameEn = "Farm", IconName = "farm" },
                new PropertyType { NameAr = "بستان", NameEn = "Orchard", IconName = "orchard" }
            );
            await context.SaveChangesAsync();
        }

        // Usage Types
        if (!await context.UsageTypes.AnyAsync())
        {
            context.UsageTypes.AddRange(
                new UsageType { NameAr = "مؤجر", NameEn = "Rented" },
                new UsageType { NameAr = "مستثمر", NameEn = "Invested" },
                new UsageType { NameAr = "خيري", NameEn = "Charitable" },
                new UsageType { NameAr = "إداري", NameEn = "Administrative" },
                new UsageType { NameAr = "فارغ", NameEn = "Vacant" },
                new UsageType { NameAr = "متنازع عليه", NameEn = "Disputed" }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedProvincesAsync(ApplicationDbContext context)
    {
        if (await context.Provinces.AnyAsync()) return;

        var provinces = new List<Province>
        {
            new Province { NameAr = "بغداد", NameEn = "Baghdad", Code = "BGW" },
            new Province { NameAr = "البصرة", NameEn = "Basra", Code = "BSR" },
            new Province { NameAr = "نينوى", NameEn = "Nineveh", Code = "NIN" },
            new Province { NameAr = "أربيل", NameEn = "Erbil", Code = "EBL" },
            new Province { NameAr = "النجف", NameEn = "Najaf", Code = "NJF" },
            new Province { NameAr = "كربلاء", NameEn = "Karbala", Code = "KBL" },
            new Province { NameAr = "الأنبار", NameEn = "Anbar", Code = "ANB" },
            new Province { NameAr = "ديالى", NameEn = "Diyala", Code = "DIY" },
            new Province { NameAr = "كركوك", NameEn = "Kirkuk", Code = "KRK" },
            new Province { NameAr = "صلاح الدين", NameEn = "Saladin", Code = "SLD" },
            new Province { NameAr = "السليمانية", NameEn = "Sulaymaniyah", Code = "SUL" },
            new Province { NameAr = "دهوك", NameEn = "Duhok", Code = "DHK" },
            new Province { NameAr = "واسط", NameEn = "Wasit", Code = "WST" },
            new Province { NameAr = "ميسان", NameEn = "Maysan", Code = "MYS" },
            new Province { NameAr = "ذي قار", NameEn = "Dhi Qar", Code = "DQR" },
            new Province { NameAr = "القادسية", NameEn = "Qadisiyyah", Code = "QDS" },
            new Province { NameAr = "بابل", NameEn = "Babylon", Code = "BBL" },
            new Province { NameAr = "المثنى", NameEn = "Muthanna", Code = "MTH" }
        };

        context.Provinces.AddRange(provinces);
        await context.SaveChangesAsync();
    }
}
