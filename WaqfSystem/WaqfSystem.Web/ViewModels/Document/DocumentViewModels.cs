using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Document;

namespace WaqfSystem.Web.ViewModels.Document
{
    public class DocumentIndexViewModel
    {
        public PagedResult<PropertyDocumentListDto> Documents { get; set; } = new();
        public DocumentFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> DocumentTypes { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public DocumentAlertSummaryVm AlertSummary { get; set; } = new();
        public string CurrentUserRole { get; set; } = string.Empty;
    }

    public class ForPropertyViewModel
    {
        public long PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public PropertyDocumentSummaryDto Summary { get; set; } = new();
        public PagedResult<PropertyDocumentListDto> Documents { get; set; } = new();
        public List<DocumentTypeGroupVm> DocumentTypeGroups { get; set; } = new();
        public DocumentFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> DocumentTypes { get; set; } = new();
        public bool CanUpload { get; set; }
        public bool CanVerify { get; set; }
    }

    public class DocumentTypeGroupVm
    {
        public DocumentTypeDto DocumentType { get; set; } = new();
        public List<PropertyDocumentListDto> Documents { get; set; } = new();
        public bool HasRequired { get; set; }
        public bool RequirementMet { get; set; }
    }

    public class DocumentDetailViewModel
    {
        public PropertyDocumentDetailDto Document { get; set; } = new();
        public bool CanVerify { get; set; }
        public bool CanUpload { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAssignResponsible { get; set; }
        public List<SelectListItem> AvailableResponsibles { get; set; } = new();
    }

    public class UploadDocumentViewModel
    {
        public UploadDocumentDto Dto { get; set; } = new();
        public long PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public List<SelectListItem> DocumentTypes { get; set; } = new();
        public List<SelectListItem> Units { get; set; } = new();
        public List<SelectListItem> Responsibles { get; set; } = new();
        public int? PreselectedTypeId { get; set; }
    }

    public class AlertsViewModel
    {
        public PagedResult<DocumentAlertDto> Alerts { get; set; } = new();
        public DocumentAlertFilterRequest Filter { get; set; } = new();
        public int UnreadCount { get; set; }
        public int ExpiredCount { get; set; }
        public int Expiring30Count { get; set; }
        public int Expiring90Count { get; set; }
    }

    public class DocumentAlertSummaryVm
    {
        public int ExpiredCount { get; set; }
        public int Expiring30 { get; set; }
        public int Expiring90 { get; set; }
    }
}
