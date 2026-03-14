using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Web.ViewModels.Properties
{
    public class PartnershipIndexViewModel
    {
        public PagedResult<PartnershipListItemDto> Partnerships { get; set; } = new();
        public PartnershipFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public PartnershipStatsDto Stats { get; set; } = new();
    }

    public class PartnershipCreateViewModel
    {
        public CreatePartnershipDto Form { get; set; } = new();
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public List<SelectListItem> Properties { get; set; } = new();
    }

    public class PartnershipDetailViewModel
    {
        public PartnershipDetailDto Partnership { get; set; } = new();
        public List<PartnerContactDto> ContactHistory { get; set; } = new();
        public List<RevenueDistributionDto> Distributions { get; set; } = new();
    }

    public class PartnershipRecordRevenueViewModel
    {
        public PartnershipDetailDto Partnership { get; set; } = new();
        public RevenueDistributionCreateDto Form { get; set; } = new();
    }

    public class PartnershipEditViewModel
    {
        public UpdatePartnershipDto Form { get; set; } = new();
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
    }

    public class PartnerListViewModel
    {
        public List<PartnerSummaryDto> Partners { get; set; } = new();
        public string? SearchTerm { get; set; }
        public PartnerType? PartnerType { get; set; }
    }

    public class PartnerCreateViewModel
    {
        public CreatePartnerDto Form { get; set; } = new();
    }
}
