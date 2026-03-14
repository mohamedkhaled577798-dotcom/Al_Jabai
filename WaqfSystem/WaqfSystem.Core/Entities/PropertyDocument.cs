using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// مستند العقار — Property document with verification and OCR support.
    /// </summary>
    public class PropertyDocument : BaseEntity
    {
        public int PropertyId { get; set; }
        public DocumentCategory DocumentCategory { get; set; } = DocumentCategory.Ownership;
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? IssuingCity { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public FileFormat FileFormat { get; set; } = FileFormat.PDF;
        public int? FileSizeKB { get; set; }
        public bool IsOriginal { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public VerificationMethod? VerificationMethod { get; set; }
        public int? VerifiedById { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? OcrText { get; set; }
        public decimal? OcrConfidence { get; set; }
        public string? GisAttachedLayerId { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual User? VerifiedBy { get; set; }
    }
}
