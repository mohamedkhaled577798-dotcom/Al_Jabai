using WaqfGIS.Core.Entities;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Models;

// تقرير المحافظة
public class ProvinceReportViewModel
{
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public int TotalMosques { get; set; }
    public int TotalProperties { get; set; }
    public int TotalOffices { get; set; }
    public int TotalCapacity { get; set; }
    public decimal TotalPropertyArea { get; set; }
    public decimal TotalPropertyValue { get; set; }
    public List<TypeCount> MosquesByType { get; set; } = new();
    public List<TypeCount> MosquesByStatus { get; set; } = new();
    public List<TypeCount> PropertiesByType { get; set; } = new();
    public List<TypeCount> PropertiesByUsage { get; set; } = new();
    public List<Mosque> Mosques { get; set; } = new();
    public List<WaqfProperty> Properties { get; set; } = new();
    public List<WaqfOffice> Offices { get; set; } = new();
}

// تقرير المساجد
public class MosquesReportViewModel
{
    public int TotalMosques { get; set; }
    public int TotalCapacity { get; set; }
    public int WithFridayPrayer { get; set; }
    public List<TypeCount> ByType { get; set; } = new();
    public List<TypeCount> ByStatus { get; set; } = new();
    public List<TypeCount> ByProvince { get; set; } = new();
    public List<Mosque> Mosques { get; set; } = new();
}

// تقرير العقارات
public class PropertiesReportViewModel
{
    public int TotalProperties { get; set; }
    public decimal TotalArea { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalRentedCount { get; set; }
    public decimal TotalMonthlyRent { get; set; }
    public List<TypeCount> ByType { get; set; } = new();
    public List<TypeCount> ByUsage { get; set; } = new();
    public List<TypeCount> ByProvince { get; set; } = new();
    public List<WaqfProperty> Properties { get; set; } = new();
}
