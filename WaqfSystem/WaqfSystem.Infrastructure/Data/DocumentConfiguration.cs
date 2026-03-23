using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Infrastructure.Data
{
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            builder.ToTable("DocumentTypes");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.NameAr).HasMaxLength(100).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.NameEn).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AllowedExtensions).HasMaxLength(200).IsRequired();
            builder.Property(x => x.VerifierRoles).HasMaxLength(200);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }

    public class PropertyDocumentConfiguration : IEntityTypeConfiguration<PropertyDocument>
    {
        public void Configure(EntityTypeBuilder<PropertyDocument> builder)
        {
            builder.ToTable("PropertyDocuments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).HasMaxLength(300).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DocumentNumber).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.IssuingAuthority).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.VerificationNotes).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.RejectionReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DeletedReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Tags).HasMaxLength(500);
            builder.Property(x => x.OcrConfidence).HasColumnType("decimal(5,2)");
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            builder.HasQueryFilter(x => !x.IsDeleted);
            builder.Ignore(x => x.DaysUntilExpiry);
            builder.Ignore(x => x.IsExpired);
            builder.Ignore(x => x.IsExpiringSoon);

            builder.HasOne(x => x.Property).WithMany(x => x.Documents).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.DocumentType).WithMany(x => x.Documents).HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CurrentVersion).WithMany().HasForeignKey(x => x.CurrentVersionId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.PrimaryResponsible).WithMany().HasForeignKey(x => x.PrimaryResponsibleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.VerifiedBy).WithMany().HasForeignKey(x => x.VerifiedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LinkedUnit).WithMany().HasForeignKey(x => x.LinkedUnitId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PropertyId, x.IsDeleted, x.Status }).HasDatabaseName("IX_PropDoc_Property");
            builder.HasIndex(x => new { x.DocumentTypeId, x.Status }).HasDatabaseName("IX_PropDoc_Type");
            builder.HasIndex(x => new { x.ExpiryDate, x.Status, x.IsDeleted }).HasDatabaseName("IX_PropDoc_Expiry").HasFilter("[ExpiryDate] IS NOT NULL");
            builder.HasIndex(x => new { x.PrimaryResponsibleId, x.Status }).HasDatabaseName("IX_PropDoc_Responsible");
        }
    }

    public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
    {
        public void Configure(EntityTypeBuilder<DocumentVersion> builder)
        {
            builder.ToTable("DocumentVersions");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
            builder.HasIndex(x => new { x.DocumentId, x.IsCurrent }).HasDatabaseName("IX_DocVersion_Doc");

            builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.FileName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
            builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.Document).WithMany(x => x.Versions).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.UploadedBy).WithMany().HasForeignKey(x => x.UploadedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DocumentAuditTrailConfiguration : IEntityTypeConfiguration<DocumentAuditTrail>
    {
        public void Configure(EntityTypeBuilder<DocumentAuditTrail> builder)
        {
            builder.ToTable("DocumentAuditTrail");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActionType).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(x => x.ActionAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.Details).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.OldValue).HasMaxLength(500);
            builder.Property(x => x.NewValue).HasMaxLength(500);

            builder.HasOne(x => x.Document).WithMany(x => x.AuditTrail).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ActionByUser).WithMany().HasForeignKey(x => x.ActionByUserId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.DocumentId, x.ActionAt }).HasDatabaseName("IX_DocAudit_Doc");
            builder.HasIndex(x => new { x.PropertyId, x.ActionAt }).HasDatabaseName("IX_DocAudit_Property");
        }
    }

    public class DocumentAlertConfiguration : IEntityTypeConfiguration<DocumentAlert>
    {
        public void Configure(EntityTypeBuilder<DocumentAlert> builder)
        {
            builder.ToTable("DocumentAlerts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AlertLevel).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.AlertType).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.NotifiedUserIds).HasMaxLength(500);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.Document).WithMany(x => x.Alerts).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ReadBy).WithMany().HasForeignKey(x => x.ReadByUserId).OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.DocumentId, x.IsRead }).HasDatabaseName("IX_DocAlert_Doc");
            builder.HasIndex(x => new { x.AlertLevel, x.IsRead, x.CreatedAt }).HasDatabaseName("IX_DocAlert_Level");
        }
    }

    public class DocumentResponsibleConfiguration : IEntityTypeConfiguration<DocumentResponsible>
    {
        public void Configure(EntityTypeBuilder<DocumentResponsible> builder)
        {
            builder.ToTable("DocumentResponsibles");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.DocumentId, x.UserId }).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.IsActive }).HasDatabaseName("IX_DocResponsible_User");

            builder.Property(x => x.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.Notes).HasMaxLength(300).UseCollation("Arabic_CI_AS");

            builder.HasOne(x => x.Document).WithMany(x => x.Responsibles).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedBy).WithMany().HasForeignKey(x => x.AssignedById).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
