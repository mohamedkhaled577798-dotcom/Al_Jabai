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

        // Seed GIS Layers
        await SeedGisLayersAsync(context);
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
                new OfficeType { NameAr = "مركز الديوان في ام القرى", NameEn = "Waqf Diwan", Level = 1 },
                new OfficeType { NameAr = "دائرة الوقف", NameEn = "Waqf Department", Level = 2 },
                new OfficeType { NameAr = "مديرية الوقف", NameEn = "Waqf Directorate", Level = 3 },
                                new OfficeType { NameAr = "ملاحظية الوقف", NameEn = "Waqf Molahdya", Level = 4 },

                new OfficeType { NameAr = "شعبة الوقف", NameEn = "Waqf Division", Level = 5 }
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
                                                new PropertyType { NameAr = "مدرسة", NameEn = "School", IconName = "School" },
                new PropertyType { NameAr = "أرض", NameEn = "Land", IconName = "land" },
                new PropertyType { NameAr = "مبنى", NameEn = "Building", IconName = "building" },
                new PropertyType { NameAr = "شقة", NameEn = "Apartment", IconName = "apartment" },
                new PropertyType { NameAr = "محل تجاري", NameEn = "Commercial Shop", IconName = "shop" },
                new PropertyType { NameAr = "مخزن", NameEn = "Warehouse", IconName = "warehouse" },
                new PropertyType { NameAr = "مزرعة", NameEn = "Farm", IconName = "farm" },
                new PropertyType { NameAr = "بستان", NameEn = "Orchard", IconName = "orchard" },
                new PropertyType { NameAr = "بيت", NameEn = "House", IconName = "House" }

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

    private static async Task SeedGisLayersAsync(ApplicationDbContext context)
    {
        if (await context.GisLayers.AnyAsync()) return;

        var layers = new List<GisLayer>
        {
            new GisLayer
            {
                Code = "MOSQUES_POINTS",
                NameAr = "المساجد (نقاط)",
                NameEn = "Mosques (Points)",
                LayerType = "Point",
                FillColor = "#1e88e5",
                StrokeColor = "#1565c0",
                FillOpacity = 0.8,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = false,
                DisplayOrder = 1,
                SourceTable = "Mosques"
            },
            new GisLayer
            {
                Code = "MOSQUES_POLYGONS",
                NameAr = "حدود المساجد",
                NameEn = "Mosque Boundaries",
                LayerType = "Polygon",
                FillColor = "#1565c0",
                StrokeColor = "#0d47a1",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = true,
                DisplayOrder = 2,
                SourceTable = "MosqueBoundaries"
            },
            new GisLayer
            {
                Code = "PROPERTIES",
                NameAr = "العقارات الوقفية",
                NameEn = "Waqf Properties",
                LayerType = "Point",
                FillColor = "#43a047",
                StrokeColor = "#2e7d32",
                FillOpacity = 0.8,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = false,
                DisplayOrder = 3,
                SourceTable = "WaqfProperties"
            },
            new GisLayer
            {
                Code = "WAQF_LANDS",
                NameAr = "أراضي الوقف",
                NameEn = "Waqf Lands",
                LayerType = "Polygon",
                FillColor = "#ff9800",
                StrokeColor = "#f57c00",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 4,
                SourceTable = "WaqfLands"
            },
            new GisLayer
            {
                Code = "ROADS",
                NameAr = "الطرق",
                NameEn = "Roads",
                LayerType = "LineString",
                FillColor = "#795548",
                StrokeColor = "#795548",
                FillOpacity = 1,
                StrokeWidth = 3,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 5,
                SourceTable = "Roads"
            },
            new GisLayer
            {
                Code = "NEARBY_PROJECTS",
                NameAr = "المشاريع المجاورة",
                NameEn = "Nearby Projects",
                LayerType = "Polygon",
                FillColor = "#9c27b0",
                StrokeColor = "#7b1fa2",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 6,
                SourceTable = "NearbyProjects"
            }
        };

        context.GisLayers.AddRange(layers);
        await context.SaveChangesAsync();
    }
}
