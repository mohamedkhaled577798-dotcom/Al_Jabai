using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Mission;

namespace WaqfSystem.Web.ViewModels.Mission
{
    public class MissionDashboardViewModel
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public List<SelectListItem> GovernorateFilter { get; set; } = new();
    }

    public class MissionIndexViewModel
    {
        public PagedResult<MissionListItemDto> Missions { get; set; } = new();
        public MissionFilterRequest Filter { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public List<SelectListItem> Employees { get; set; } = new();
        public string CurrentUserRole { get; set; } = string.Empty;
        public int TotalActive { get; set; }
        public int TotalOverdue { get; set; }
        public int TotalCompletedThisMonth { get; set; }
    }

    public class MissionCreateViewModel
    {
        public CreateMissionDto Mission { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public List<SelectListItem> Employees { get; set; } = new();
        public List<SelectListItem> Teams { get; set; } = new();
        public List<SelectListItem> ChecklistTemplates { get; set; } = new();
        public List<SelectListItem> MissionTypeOptions { get; set; } = new();
        public List<SelectListItem> PriorityOptions { get; set; } = new();
    }

    public class MissionDetailViewModel
    {
        public MissionDetailDto Mission { get; set; } = new();
        public int CurrentUserId { get; set; }
        public string CurrentUserRole { get; set; } = string.Empty;
    }

    public class MissionCalendarViewModel
    {
        public List<CalendarEventDto> Events { get; set; } = new();
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthNameAr { get; set; } = string.Empty;
        public int PrevYear { get; set; }
        public int PrevMonth { get; set; }
        public int NextYear { get; set; }
        public int NextMonth { get; set; }
        public List<SelectListItem> Governorates { get; set; } = new();
        public int? FilterGovernorateId { get; set; }
    }

    public class PerformanceReportSummaryViewModel
    {
        public int TotalEmployees { get; set; }
        public decimal AvgCompletionRate { get; set; }
        public decimal AvgDqsScore { get; set; }
        public int TotalCompleted { get; set; }
    }

    public class PerformanceReportViewModel
    {
        public List<EmployeePerformanceDto> Employees { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public int? FilterGovernorateId { get; set; }
        public DateTime FilterFrom { get; set; }
        public DateTime FilterTo { get; set; }
        public PerformanceReportSummaryViewModel Summary { get; set; } = new();
    }

    public class TeamsViewModel
    {
        public List<TeamDetailDto> Teams { get; set; } = new();
        public List<SelectListItem> Governorates { get; set; } = new();
        public int? FilterGovernorateId { get; set; }
    }

    public class TeamDetailViewModel
    {
        public TeamDetailDto Team { get; set; } = new();
        public List<SelectListItem> AvailableEmployees { get; set; } = new();
        public List<MissionListItemDto> RecentMissions { get; set; } = new();
    }
}
