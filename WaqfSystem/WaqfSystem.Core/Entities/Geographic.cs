using System;
using System.Collections.Generic;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// Geographic hierarchy entities for Iraq (7 levels).
    /// Countries → Governorates → Districts → SubDistricts → Neighborhoods → Streets
    /// </summary>

    /// <summary>بلد</summary>
    public class Country
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
    }

    /// <summary>محافظة</summary>
    public class Governorate
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Country Country { get; set; } = null!;
        public virtual ICollection<District> Districts { get; set; } = new List<District>();
    }

    /// <summary>قضاء</summary>
    public class District
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Governorate Governorate { get; set; } = null!;
        public virtual ICollection<SubDistrict> SubDistricts { get; set; } = new List<SubDistrict>();
    }

    /// <summary>ناحية</summary>
    public class SubDistrict
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual District District { get; set; } = null!;
        public virtual ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }

    /// <summary>حي / محلة</summary>
    public class Neighborhood
    {
        public int Id { get; set; }
        public int SubDistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual SubDistrict SubDistrict { get; set; } = null!;
        public virtual ICollection<Street> Streets { get; set; } = new List<Street>();
    }

    /// <summary>شارع</summary>
    public class Street
    {
        public int Id { get; set; }
        public int NeighborhoodId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GisLayerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Neighborhood Neighborhood { get; set; } = null!;
    }
}
