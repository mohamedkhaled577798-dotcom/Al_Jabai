using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Document
{
    public class DocumentTypeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool HasExpiry { get; set; }
        public int? AlertDays1 { get; set; }
        public int? AlertDays2 { get; set; }
        public string AllowedExtensions { get; set; } = string.Empty;
        public List<string> VerifierRoles { get; set; } = new();
        public bool IsActive { get; set; }
        public int DocumentCount { get; set; }
    }

    public class PropertyDocumentListDto
    {
        public long Id { get; set; }
        public long PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public int DocumentTypeId { get; set; }
        public string DocumentTypeNameAr { get; set; } = string.Empty;
        public string DocumentTypeCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public string? IssuingAuthority { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string StatusDisplayAr { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#378ADD";
        public int? DaysUntilExpiry { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
        public int VersionCount { get; set; }
        public long? CurrentVersionId { get; set; }
        public string? CurrentFileName { get; set; }
        public long? CurrentFileSizeBytes { get; set; }
        public string? PrimaryResponsibleName { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool HasOcr { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int UnreadAlertCount { get; set; }
    }

    public class PropertyDocumentDetailDto : PropertyDocumentListDto
    {
        public string? Description { get; set; }
        public string? OcrText { get; set; }
        public decimal? OcrConfidence { get; set; }
        public long? LinkedUnitId { get; set; }
        public string? LinkedUnitNumber { get; set; }
        public long? LinkedPartnershipId { get; set; }
        public string? VerificationNotes { get; set; }
        public string? RejectionReason { get; set; }
        public List<DocumentVersionDto> Versions { get; set; } = new();
        public List<DocumentAuditTrailDto> AuditTrail { get; set; } = new();
        public List<DocumentAlertDto> ActiveAlerts { get; set; } = new();
        public List<DocumentResponsibleDto> Responsibles { get; set; } = new();
        public DocumentTypeDto? DocumentType { get; set; }
        public List<string> AllowedNextActions { get; set; } = new();
    }

    public class DocumentVersionDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileSizeDisplay { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int? PageCount { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsCurrent { get; set; }
        public string? Notes { get; set; }
    }

    public class DocumentAuditTrailDto
    {
        public long Id { get; set; }
        public DocumentActionType ActionType { get; set; }
        public string ActionTypeDisplayAr { get; set; } = string.Empty;
        public string? ActionByName { get; set; }
        public DateTime ActionAt { get; set; }
        public string? Details { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? VersionNumber { get; set; }
    }

    public class DocumentAlertDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public string DocumentTypeNameAr { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int? DaysRemaining { get; set; }
        public DocumentAlertLevel AlertLevel { get; set; }
        public DocumentAlertType AlertType { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UrgencyColor { get; set; } = "#378ADD";
        public string UrgencyLabel { get; set; } = string.Empty;
    }

    public class DocumentResponsibleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullNameAr { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class UploadDocumentDto
    {
        [Required]
        public long PropertyId { get; set; }

        [Required]
        public int DocumentTypeId { get; set; }

        [Required]
        [StringLength(300, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? DocumentNumber { get; set; }
        public string? IssuingAuthority { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public long? LinkedUnitId { get; set; }
        public long? LinkedPartnershipId { get; set; }
        public int? PrimaryResponsibleId { get; set; }
        public List<string>? Tags { get; set; }
        public string? Notes { get; set; }

        // Legacy fields kept for backward compatibility with existing controllers.
        public DocumentCategory? DocumentCategory { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DocumentDate { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class UploadNewVersionDto
    {
        [Required]
        public long DocumentId { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;

        public string? Notes { get; set; }
        public DateTime? UpdateExpiryDate { get; set; }
    }

    public class VerifyDocumentDto
    {
        [Required]
        public long DocumentId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string? Notes { get; set; }
    }

    public class DocumentFilterRequest
    {
        public long? PropertyId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? Category { get; set; }
        public DocumentStatus? Status { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsExpiringSoon { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
        public int? PrimaryResponsibleId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDesc { get; set; } = true;
    }

    public class DocumentAlertFilterRequest
    {
        public long? PropertyId { get; set; }
        public int? AlertLevel { get; set; }
        public bool? IsRead { get; set; }
        public int? DocumentTypeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PropertyDocumentSummaryDto
    {
        public long PropertyId { get; set; }
        public int TotalDocuments { get; set; }
        public int VerifiedCount { get; set; }
        public int PendingCount { get; set; }
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int ExpiringSoon90Count { get; set; }
        public List<string> MissingRequiredTypes { get; set; } = new();
        public decimal CompliancePercent { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }

    public class CreateDocumentTypeDto
    {
        [Required]
        [StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Category { get; set; } = string.Empty;

        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool HasExpiry { get; set; }
        public int? AlertDays1 { get; set; }
        public int? AlertDays2 { get; set; }
        public string AllowedExtensions { get; set; } = "pdf,jpg,jpeg,png,tiff";
        public int MaxFileSizeMB { get; set; } = 10;
        public List<string> VerifierRoles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdateDocumentTypeDto : CreateDocumentTypeDto
    {
        [Required]
        public int Id { get; set; }
    }
}
