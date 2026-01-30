using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Infrastructure.Data;

namespace WaqfGIS.Services.GIS;

/// <summary>
/// بيانات تجريبية لطبقات GIS
/// </summary>
public static class GisDataSeeder
{
    public static async Task SeedGisLayersAsync(ApplicationDbContext context)
    {
        if (await context.GisLayers.AnyAsync())
            return;

        var layers = new List<GisLayer>
        {
            new GisLayer
            {
                Code = "MOSQUES_POINTS",
                NameAr = "المساجد (نقاط)",
                NameEn = "Mosques (Points)",
                LayerType = "Point",
                Description = "مواقع المساجد والجوامع",
                FillColor = "#1e88e5",
                StrokeColor = "#1565c0",
                FillOpacity = 0.8,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = false,
                DisplayOrder = 1,
                MinZoom = 8,
                SourceTable = "Mosques",
                GeometryColumn = "Location"
            },
            new GisLayer
            {
                Code = "MOSQUES_POLYGONS",
                NameAr = "حدود المساجد",
                NameEn = "Mosque Boundaries",
                LayerType = "Polygon",
                Description = "حدود مباني المساجد",
                FillColor = "#1565c0",
                StrokeColor = "#0d47a1",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = true,
                DisplayOrder = 2,
                MinZoom = 14,
                SourceTable = "MosqueBoundaries",
                GeometryColumn = "Boundary"
            },
            new GisLayer
            {
                Code = "PROPERTIES",
                NameAr = "العقارات الوقفية",
                NameEn = "Waqf Properties",
                LayerType = "Point",
                Description = "مواقع العقارات الوقفية",
                FillColor = "#43a047",
                StrokeColor = "#2e7d32",
                FillOpacity = 0.8,
                StrokeWidth = 2,
                IsVisible = true,
                IsEditable = false,
                DisplayOrder = 3,
                MinZoom = 10,
                SourceTable = "WaqfProperties",
                GeometryColumn = "Location"
            },
            new GisLayer
            {
                Code = "WAQF_LANDS",
                NameAr = "أراضي الوقف",
                NameEn = "Waqf Lands",
                LayerType = "Polygon",
                Description = "حدود أراضي الوقف",
                FillColor = "#ff9800",
                StrokeColor = "#f57c00",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 4,
                MinZoom = 12,
                SourceTable = "WaqfLands",
                GeometryColumn = "Boundary"
            },
            new GisLayer
            {
                Code = "ROADS",
                NameAr = "الطرق",
                NameEn = "Roads",
                LayerType = "LineString",
                Description = "شبكة الطرق",
                FillColor = "#795548",
                StrokeColor = "#795548",
                FillOpacity = 1,
                StrokeWidth = 3,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 5,
                MinZoom = 14,
                SourceTable = "Roads",
                GeometryColumn = "Geometry"
            },
            new GisLayer
            {
                Code = "NEARBY_PROJECTS",
                NameAr = "المشاريع المجاورة",
                NameEn = "Nearby Projects",
                LayerType = "Polygon",
                Description = "المشاريع القريبة من أملاك الوقف",
                FillColor = "#9c27b0",
                StrokeColor = "#7b1fa2",
                FillOpacity = 0.3,
                StrokeWidth = 2,
                IsVisible = false,
                IsEditable = true,
                DisplayOrder = 6,
                MinZoom = 12,
                SourceTable = "NearbyProjects",
                GeometryColumn = "Boundary"
            }
        };

        await context.GisLayers.AddRangeAsync(layers);
        await context.SaveChangesAsync();
    }
}
