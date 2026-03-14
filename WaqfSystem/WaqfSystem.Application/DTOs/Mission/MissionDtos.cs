using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Mission
{
    public class CreateMissionDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public MissionType MissionType { get; set; }

        [Required]
        public DateTime MissionDate { get; set; }

        [Required]
        public int GovernorateId { get; set; }

        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? TargetArea { get; set; }

        [Range(0, 99999)]
        public int TargetPropertyCount { get; set; }

        public string? Description { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }

        [Required]
        public MissionPriority Priority { get; set; }

        public bool IsUrgent { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public string? AssignmentNotes { get; set; }
        public int? ReviewerUserId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public List<long>? PropertyIds { get; set; }
    }

    public class UpdateMissionDto
    {
        [Required]
        public int Id { get; set; }

        public string? Title { get; set; }
        public MissionType? MissionType { get; set; }
        public DateTime? MissionDate { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? TargetArea { get; set; }
        public int? TargetPropertyCount { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public MissionPriority? Priority { get; set; }
        public bool? IsUrgent { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public string? AssignmentNotes { get; set; }
        public int? ReviewerUserId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public List<long>? PropertyIds { get; set; }
    }

    public class AssignMissionDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public int AssignedToUserId { get; set; }

        public int? AssignedToTeamId { get; set; }
        public string? AssignmentNotes { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public int? ReviewerUserId { get; set; }
    }

    public class ReassignMissionDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public int NewUserId { get; set; }

        [Required]
        public string ReassignReason { get; set; } = string.Empty;
    }

    public class AdvanceStageDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public MissionStage ToStage { get; set; }

        public string? Notes { get; set; }
        public decimal? CheckinLat { get; set; }
        public decimal? CheckinLng { get; set; }
    }

    public class SubmitForReviewDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public string InspectorNotes { get; set; } = string.Empty;

        public List<ChecklistItemResult>? ChecklistResults { get; set; }
    }

    public class ChecklistItemResult
    {
        public int ItemId { get; set; }
        public string Answer { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class ApproveRejectDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [Required]
        public string Notes { get; set; } = string.Empty;
    }

    public class CancelMissionDto
    {
        [Required]
        public int MissionId { get; set; }

        [Required]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class MissionListItemDto
    {
        public int Id { get; set; }
        public string MissionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public MissionType MissionType { get; set; }
        public MissionStage Stage { get; set; }
        public MissionPriority Priority { get; set; }
        public string GovernorateNameAr { get; set; } = string.Empty;
        public string? DistrictNameAr { get; set; }
        public string? AssignedToName { get; set; }
        public string AssignedToAvatar { get; set; } = "--";
        public DateTime MissionDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public int TargetPropertyCount { get; set; }
        public int EnteredPropertyCount { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal? AverageDqsScore { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
        public string StageColor { get; set; } = "#6c757d";
        public string StageDisplayAr { get; set; } = string.Empty;
    }

    public class MissionDetailDto : MissionListItemDto
    {
        public string? Description { get; set; }
        public string? TargetArea { get; set; }
        public UserBriefDto? AssignedToUser { get; set; }
        public UserBriefDto? AssignedByUser { get; set; }
        public UserBriefDto? ReviewerUser { get; set; }
        public TeamBriefDto? Team { get; set; }
        public List<MissionStageHistoryDto> StageHistory { get; set; } = new();
        public List<MissionPropertyEntryDto> PropertyEntries { get; set; } = new();
        public ChecklistTemplateDto? ChecklistTemplate { get; set; }
        public List<ChecklistResultDto> ChecklistResults { get; set; } = new();
        public MissionProgressStatsDto ProgressStats { get; set; } = new();
        public List<MissionStage> AllowedNextStages { get; set; } = new();
        public bool CanAccept { get; set; }
        public bool CanReject { get; set; }
        public bool CanCheckin { get; set; }
        public bool CanSubmitReview { get; set; }
        public bool CanApprove { get; set; }
        public bool CanSendBack { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReassign { get; set; }
    }

    public class MissionProgressStatsDto
    {
        public int TargetCount { get; set; }
        public int EnteredCount { get; set; }
        public int ReviewedCount { get; set; }
        public int ApprovedCount { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal AverageDqsScore { get; set; }
        public int Below50DqsCount { get; set; }
        public int Below70DqsCount { get; set; }
        public bool IsOnSchedule { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class MissionStageHistoryDto
    {
        public MissionStage? FromStage { get; set; }
        public MissionStage ToStage { get; set; }
        public string FromStageAr { get; set; } = string.Empty;
        public string ToStageAr { get; set; } = string.Empty;
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
        public string? TriggerAction { get; set; }
    }

    public class MissionPropertyEntryDto
    {
        public long Id { get; set; }
        public int? PropertyId { get; set; }
        public string? LocalId { get; set; }
        public string? PropertyNameAr { get; set; }
        public string? PropertyWqfNumber { get; set; }
        public string EnteredByName { get; set; } = string.Empty;
        public EntryStatus EntryStatus { get; set; }
        public decimal? DqsAtEntry { get; set; }
        public DateTime EntryStartedAt { get; set; }
        public DateTime? EntryCompletedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public string? ReviewedByName { get; set; }
    }

    public class MissionFilterRequest
    {
        public int? GovernorateId { get; set; }
        public MissionStage? Stage { get; set; }
        public MissionType? MissionType { get; set; }
        public int? AssignedToUserId { get; set; }
        public MissionPriority? Priority { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsUrgent { get; set; }
        public bool? IsOverdue { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "MissionDate";
        public bool SortDesc { get; set; } = true;
    }

    public class UserBriefDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? GovernorateId { get; set; }
        public string? Phone { get; set; }
        public string AvatarInitials { get; set; } = "--";
    }

    public class TeamBriefDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
    }

    public class ChecklistTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public List<ChecklistItemDefinitionDto> Items { get; set; } = new();
    }

    public class ChecklistItemDefinitionDto
    {
        public int Id { get; set; }
        public string QuestionAr { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
    }

    public class ChecklistResultDto
    {
        public int TemplateId { get; set; }
        public decimal CompletionPercent { get; set; }
        public List<ChecklistItemResult> Results { get; set; } = new();
        public string CompletedByName { get; set; } = string.Empty;
    }

    public class TeamCreateDto
    {
        public string TeamName { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public int GovernorateId { get; set; }
        public int? LeaderId { get; set; }
        public string? Description { get; set; }
    }

    public class TeamDetailDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public int GovernorateId { get; set; }
        public string GovernorateNameAr { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActiveMissionCount { get; set; }
        public List<TeamMemberDto> Members { get; set; } = new();
    }

    public class TeamMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public int ActiveMissionCount { get; set; }
    }

    public class EmployeePerformanceDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string GovernorateNameAr { get; set; } = string.Empty;
        public int TotalMissionsAssigned { get; set; }
        public int CompletedMissions { get; set; }
        public int InProgressMissions { get; set; }
        public decimal AverageDqsScore { get; set; }
        public decimal AverageCompletionDays { get; set; }
        public decimal OnTimeCompletionRate { get; set; }
        public int TotalPropertiesEntered { get; set; }
        public int ApprovedPropertiesCount { get; set; }
        public DateTime? LastMissionDate { get; set; }
        public string PerformanceRating { get; set; } = "متوسط";
    }

    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public MissionType MissionType { get; set; }
        public MissionStage Stage { get; set; }
        public MissionPriority Priority { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public string GovernorateNameAr { get; set; } = string.Empty;
        public string Color { get; set; } = "#6c757d";
        public bool IsUrgent { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalMissions { get; set; }
        public int ActiveMissions { get; set; }
        public int CompletedThisMonth { get; set; }
        public int OverdueMissions { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public decimal AverageDqsScore { get; set; }
        public int TotalPropertiesEnteredThisMonth { get; set; }
        public Dictionary<string, int> MissionsByStage { get; set; } = new();
        public Dictionary<string, int> MissionsByType { get; set; } = new();
        public List<EmployeePerformanceDto> TopPerformers { get; set; } = new();
        public List<MissionStageHistoryDto> RecentActivity { get; set; } = new();
        public List<MissionListItemDto> UpcomingMissions { get; set; } = new();
    }
}
