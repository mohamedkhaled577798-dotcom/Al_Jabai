using System;
using System.Collections.Generic;

namespace WaqfSystem.Application.DTOs.Mobile
{
    public class MobileLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? DeviceModel { get; set; }
        public string? AppVersion { get; set; }
    }

    public class MobileAuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public MobileUserDto User { get; set; } = null!;
        public bool SyncRequired { get; set; }
    }

    public class MobileUserDto
    {
        public int Id { get; set; }
        public string FullNameAr { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? GovernorateId { get; set; }
        public string? GovernorateName { get; set; }
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
    }

    public class SyncPushDto
    {
        public string? DeviceId { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public List<SyncPropertyItem> Properties { get; set; } = new();
        public List<SyncPhotoItem> Photos { get; set; } = new();
        public List<SyncDocumentItem> Documents { get; set; } = new();
    }

    public class SyncPropertyItem
    {
        public string LocalId { get; set; } = string.Empty;
        public int? ServerId { get; set; }
        public string JsonData { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }
    }

    public class SyncPhotoItem
    {
        public string LocalId { get; set; } = string.Empty;
        public string PropertyLocalId { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public DateTime CapturedAt { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class SyncDocumentItem
    {
        public string LocalId { get; set; } = string.Empty;
        public string PropertyLocalId { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string? DocumentType { get; set; }
    }

    public class SyncPullResponseDto
    {
        public List<SyncPropertyItem> Properties { get; set; } = new();
        public List<string> DeletedPropertyIds { get; set; } = new();
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    }

    public class SyncPushResponseDto
    {
        public List<SyncedIdMapping> SyncedIds { get; set; } = new();
        public List<SyncConflict> Conflicts { get; set; } = new();
        public DateTime NextSyncAt { get; set; } = DateTime.UtcNow;
    }

    public class SyncedIdMapping
    {
        public string LocalId { get; set; } = string.Empty;
        public int ServerId { get; set; }
    }

    public class SyncConflict
    {
        public string LocalId { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty;
        public string? ServerVersion { get; set; }
    }

    public class InitialSyncDto
    {
        public List<GovernorateItem> Governorates { get; set; } = new();
        public List<DistrictItem> Districts { get; set; } = new();
        public List<SubDistrictItem> SubDistricts { get; set; } = new();
        public List<NeighborhoodItem> Neighborhoods { get; set; } = new();
        public List<LookupItem> PropertyTypes { get; set; } = new();
        public List<LookupItem> Roles { get; set; } = new();
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    }

    public class GovernorateItem
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class DistrictItem
    {
        public int Id { get; set; }
        public int GovernorateId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class SubDistrictItem
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
    }

    public class NeighborhoodItem
    {
        public int Id { get; set; }
        public int SubDistrictId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? Code { get; set; }
    }

    public class GisSubmitPolygonDto
    {
        public int? PropertyId { get; set; }
        public string? LocalId { get; set; }
        public string GeoJson { get; set; } = string.Empty;
        public decimal? Accuracy { get; set; }
        public DateTime? CapturedAt { get; set; }
        public decimal? MeasuredArea { get; set; }
    }
}
