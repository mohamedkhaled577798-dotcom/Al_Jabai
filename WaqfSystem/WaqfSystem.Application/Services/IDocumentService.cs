using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Services
{
    public interface IDocumentService
    {
        Task<long> UploadAsync(UploadDocumentDto dto, int userId);
        Task<long> UploadAsync(IFormFile file, UploadDocumentDto dto, int userId);
        Task<long> UploadAsync(IFormFile file, WaqfSystem.Application.DTOs.Property.UploadDocumentDto dto, int userId);
        Task<DocumentVersionDto> UploadNewVersionAsync(UploadNewVersionDto dto, int userId);
        Task VerifyAsync(VerifyDocumentDto dto, int userId);
        Task VerifyAsync(int id, int userId, VerificationMethod method, string? notes);
        Task DeleteAsync(int id, int userId);
        Task<long> AddPhotoAsync(IFormFile file, int propertyId, PhotoType photoType, int userId);
        Task SoftDeleteAsync(long id, string reason, int userId);
        Task RestoreAsync(long id, int userId);
        Task AssignResponsibleAsync(long documentId, int userId, string? notes, int assignedById);
        Task RemoveResponsibleAsync(long documentId, int userId, int removedById);
        Task RecordDownloadAsync(long documentId, long versionId, int userId);
        Task RecordViewAsync(long documentId, int userId);
        Task ProcessOcrAsync(long documentId);
        Task<PagedResult<PropertyDocumentListDto>> GetByPropertyAsync(long propertyId, DocumentFilterRequest filter);
        Task<PropertyDocumentDetailDto?> GetDetailAsync(long id, int userId);
        Task<PagedResult<PropertyDocumentListDto>> SearchAsync(DocumentFilterRequest filter, int userId);
        Task<PropertyDocumentSummaryDto> GetPropertySummaryAsync(long propertyId);
        Task<PagedResult<DocumentAlertDto>> GetAlertsAsync(DocumentAlertFilterRequest filter, int userId);
        Task MarkAlertReadAsync(long alertId, int userId);
        Task MarkAllAlertsReadAsync(long? propertyId, int userId);

        Task<List<DocumentTypeDto>> GetDocumentTypesAsync();
        Task<List<DocumentAuditTrailDto>> GetAuditTrailAsync(long documentId, int take = 100);
    }
}
