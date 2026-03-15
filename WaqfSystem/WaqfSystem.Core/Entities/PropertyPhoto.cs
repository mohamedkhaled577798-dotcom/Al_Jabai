using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// صورة العقار — Property photo with GPS metadata.
    /// </summary>
    public class PropertyPhoto : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public PhotoType PhotoType { get; set; } = PhotoType.FrontFacade;
        public string FileUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int? FileSizeKB { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? TakenAt { get; set; }
        public decimal? DeviceAccuracy { get; set; }
        public bool IsMain { get; set; } = false;
        public string? Caption { get; set; }
        public int UploadedById { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual PropertyUnit? Unit { get; set; }
        public virtual User UploadedBy { get; set; } = null!;
    }
}
