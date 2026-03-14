using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IDocumentService
    {
        Task<DocumentDto> UploadAsync(IFormFile file, UploadDocumentDto dto, int userId);
        Task<List<DocumentDto>> GetByPropertyIdAsync(int propertyId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> VerifyAsync(int id, int userId, VerificationMethod method, string notes);
        Task<DocumentDto> AddPhotoAsync(IFormFile file, int propertyId, PhotoType type, int userId, int? unitId = null, string? caption = null);
        Task<List<PhotoDto>> GetPhotosByPropertyIdAsync(int propertyId);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;
        private readonly IOcrService _ocrService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFileStorageService fileStorage,
            IOcrService ocrService,
            ILogger<DocumentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _ocrService = ocrService;
            _logger = logger;
        }

        public async Task<DocumentDto> UploadAsync(IFormFile file, UploadDocumentDto dto, int userId)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(dto.PropertyId);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {dto.PropertyId}");

            // 1. Upload file
            var fileUrl = await _fileStorage.UploadFileAsync(file, "documents");

            // 2. Map and save entity
            var document = _mapper.Map<PropertyDocument>(dto);
            document.FileUrl = fileUrl;
            document.CreatedById = userId;
            document.CreatedAt = DateTime.UtcNow;
            document.FileSizeKB = (int)(file.Length / 1024);

            // 3. Optional OCR
            try
            {
                var (text, confidence) = await _ocrService.ExtractTextAsync(fileUrl);
                document.OcrText = text;
                document.OcrConfidence = confidence;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OCR failed for document {FileUrl}", fileUrl);
            }

            await _unitOfWork.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document uploaded: {FileUrl} for property {PropertyId}", fileUrl, dto.PropertyId);
            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<List<DocumentDto>> GetByPropertyIdAsync(int propertyId)
        {
            var docs = await _unitOfWork.Properties.GetDocumentsByPropertyIdAsync(propertyId);
            return _mapper.Map<List<DocumentDto>>(docs);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var doc = await _unitOfWork.GetByIdAsync<PropertyDocument>(id);
            if (doc == null) return false;

            // Delete from storage
            await _fileStorage.DeleteFileAsync(doc.FileUrl);

            // Delete record
            await _unitOfWork.DeleteAsync(doc);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document deleted: {Id} by user {UserId}", id, userId);
            return true;
        }

        public async Task<bool> VerifyAsync(int id, int userId, VerificationMethod method, string notes)
        {
            var doc = await _unitOfWork.GetByIdAsync<PropertyDocument>(id);
            if (doc == null) return false;

            doc.IsVerified = true;
            doc.VerifiedById = userId;
            doc.VerifiedAt = DateTime.UtcNow;
            doc.VerificationMethod = method;
            doc.Notes += $"\nVerification Notes: {notes}";

            await _unitOfWork.UpdateAsync(doc);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<DocumentDto> AddPhotoAsync(IFormFile file, int propertyId, PhotoType type, int userId, int? unitId = null, string? caption = null)
        {
             var property = await _unitOfWork.Properties.GetByIdAsync(propertyId);
            if (property == null)
                throw new InvalidOperationException($"العقار غير موجود: {propertyId}");

            var fileUrl = await _fileStorage.UploadFileAsync(file, "photos");
            var thumbUrl = await _fileStorage.GenerateThumbnailAsync(fileUrl);

            var photo = new PropertyPhoto
            {
                PropertyId = propertyId,
                UnitId = unitId,
                PhotoType = type,
                FileUrl = fileUrl,
                ThumbnailUrl = thumbUrl,
                FileSizeKB = (int)(file.Length / 1024),
                UploadedById = userId,
                Caption = caption,
                CreatedAt = DateTime.UtcNow,
                IsMain = (await _unitOfWork.Properties.GetPhotosByPropertyIdAsync(propertyId)).Count() == 0
            };

            await _unitOfWork.AddAsync(photo);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DocumentDto>(photo); // Mapping to DocumentDto for simplicity in some contexts, but ideally use PhotoDto
        }

        public async Task<List<PhotoDto>> GetPhotosByPropertyIdAsync(int propertyId)
        {
            var photos = await _unitOfWork.Properties.GetPhotosByPropertyIdAsync(propertyId);
            return _mapper.Map<List<PhotoDto>>(photos);
        }
    }
}
