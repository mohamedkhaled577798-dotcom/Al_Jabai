using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaqfSystem.Core.Entities
{
    public enum DocumentStatus
    {
        PendingVerification,
        Verified,
        Rejected,
        ExpiringSoon,
        Expired,
        Archived
    }

    public enum DocumentActionType
    {
        Uploaded,
        Downloaded,
        Verified,
        Rejected,
        NewVersionUploaded,
        ResponsibleAssigned,
        ExpiryChanged,
        AlertCreated,
        AlertRead,
        SoftDeleted,
        Restored,
        OcrProcessed,
        LinkedToUnit,
        LinkedToPartnership,
        Viewed
    }

    public enum DocumentAlertLevel
    {
        Day90 = 1,
        Day30 = 2,
        Expired = 3
    }

    public enum DocumentAlertType
    {
        ExpiringSoon,
        Expired,
        PendingVerification
    }

    public class DocumentType
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool HasExpiry { get; set; }
        public int? AlertDays1 { get; set; } = 90;
        public int? AlertDays2 { get; set; } = 30;
        public string AllowedExtensions { get; set; } = "pdf,jpg,jpeg,png,tiff";
        public int MaxFileSizeMB { get; set; } = 10;
        public string? VerifierRoles { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }

        public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    }

    public class PropertyDocument
    {
        public long Id { get; set; }
        public int PropertyId { get; set; }
        public int DocumentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IssuingAuthority { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.PendingVerification;
        public long? CurrentVersionId { get; set; }
        public int VersionCount { get; set; }
        public int? LinkedUnitId { get; set; }
        public long? LinkedPartnershipId { get; set; }
        public int? PrimaryResponsibleId { get; set; }
        public int? VerifiedById { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNotes { get; set; }
        public string? RejectionReason { get; set; }
        public string? OcrText { get; set; }
        public decimal? OcrConfidence { get; set; }
        public DateTime? OcrProcessedAt { get; set; }
        public bool Alert1Sent { get; set; }
        public bool Alert2Sent { get; set; }
        public bool ExpiredAlertSent { get; set; }
        public string? Tags { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public string? DeletedReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Property Property { get; set; } = null!;
        public virtual DocumentType DocumentType { get; set; } = null!;
        public virtual DocumentVersion? CurrentVersion { get; set; }
        public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
        public virtual ICollection<DocumentAuditTrail> AuditTrail { get; set; } = new List<DocumentAuditTrail>();
        public virtual ICollection<DocumentAlert> Alerts { get; set; } = new List<DocumentAlert>();
        public virtual ICollection<DocumentResponsible> Responsibles { get; set; } = new List<DocumentResponsible>();
        public virtual User? PrimaryResponsible { get; set; }
        public virtual User? VerifiedBy { get; set; }
        public virtual PropertyUnit? LinkedUnit { get; set; }
        public virtual User CreatedBy { get; set; } = null!;

        [NotMapped]
        public int? DaysUntilExpiry => ExpiryDate.HasValue ? (ExpiryDate.Value.Date - DateTime.Today).Days : null;

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;

        [NotMapped]
        public bool IsExpiringSoon => DaysUntilExpiry.HasValue && DaysUntilExpiry.Value <= 90 && DaysUntilExpiry.Value > 0;
    }

    public class DocumentVersion
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int? PageCount { get; set; }
        public int UploadedById { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsCurrent { get; set; } = true;
        public string? Notes { get; set; }

        public virtual PropertyDocument Document { get; set; } = null!;
        public virtual User UploadedBy { get; set; } = null!;
    }

    public class DocumentAuditTrail
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int PropertyId { get; set; }
        public DocumentActionType ActionType { get; set; }
        public int? ActionByUserId { get; set; }
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
        public long? VersionId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public virtual PropertyDocument Document { get; set; } = null!;
        public virtual User? ActionByUser { get; set; }
        public virtual DocumentVersion? Version { get; set; }
    }

    public class DocumentAlert
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int PropertyId { get; set; }
        public DocumentAlertLevel AlertLevel { get; set; }
        public DocumentAlertType AlertType { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? ReadByUserId { get; set; }
        public string? NotifiedUserIds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual PropertyDocument Document { get; set; } = null!;
        public virtual User? ReadBy { get; set; }
    }

    public class DocumentResponsible
    {
        public int Id { get; set; }
        public long DocumentId { get; set; }
        public int UserId { get; set; }
        public int AssignedById { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        public virtual PropertyDocument Document { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User AssignedBy { get; set; } = null!;
    }
}
