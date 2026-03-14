using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    public class InspectionTeam
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public int GovernorateId { get; set; }
        public int? LeaderId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedById { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Governorate Governorate { get; set; } = null!;
        public virtual User? Leader { get; set; }
        public virtual ICollection<InspectionTeamMember> Members { get; set; } = new List<InspectionTeamMember>();
        public virtual ICollection<InspectionMission> Missions { get; set; } = new List<InspectionMission>();
    }

    public class InspectionTeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public int AddedById { get; set; }

        public virtual InspectionTeam Team { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User AddedBy { get; set; } = null!;
    }

    public class InspectionMission
    {
        public int Id { get; set; }
        public string MissionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public MissionType MissionType { get; set; } = MissionType.PropertyCensus;
        public MissionStage Stage { get; set; } = MissionStage.Created;
        public MissionPriority Priority { get; set; } = MissionPriority.Normal;
        public int GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? TargetArea { get; set; }
        public int TargetPropertyCount { get; set; }
        public int EnteredPropertyCount { get; set; }
        public int ReviewedPropertyCount { get; set; }
        public int ApprovedPropertyCount { get; set; }
        public decimal? AverageDqsScore { get; set; }
        public decimal ProgressPercent { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int? ReviewerUserId { get; set; }
        public DateTime MissionDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public decimal? CheckinLat { get; set; }
        public decimal? CheckinLng { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsUrgent { get; set; }
        public string? AssignmentNotes { get; set; }
        public string? ReviewNotes { get; set; }
        public string? CancellationReason { get; set; }
        public string? RejectionReason { get; set; }
        public string? CorrectionNotes { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public DateTime CurrentStageChangedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Governorate Governorate { get; set; } = null!;
        public virtual District? District { get; set; }
        public virtual SubDistrict? SubDistrict { get; set; }
        public virtual User? AssignedToUser { get; set; }
        public virtual User? AssignedByUser { get; set; }
        public virtual User? ReviewerUser { get; set; }
        public virtual InspectionTeam? AssignedToTeam { get; set; }
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<MissionStageHistory> StageHistory { get; set; } = new List<MissionStageHistory>();
        public virtual ICollection<MissionPropertyEntry> PropertyEntries { get; set; } = new List<MissionPropertyEntry>();
        public virtual ICollection<MissionChecklistResult> ChecklistResults { get; set; } = new List<MissionChecklistResult>();
        public virtual MissionChecklistTemplate? ChecklistTemplate { get; set; }

        [NotMapped]
        public bool IsOverdue => ExpectedCompletionDate.HasValue && DateTime.Today > ExpectedCompletionDate.Value.Date && Stage != MissionStage.Completed && Stage != MissionStage.Cancelled;

        [NotMapped]
        public int DaysRemaining => ExpectedCompletionDate.HasValue ? (ExpectedCompletionDate.Value.Date - DateTime.Today).Days : 0;

        // Legacy compatibility aliases for existing pages/controllers.
        [NotMapped]
        public string MissionNumber
        {
            get => MissionCode;
            set => MissionCode = value;
        }

        [NotMapped]
        public MissionStatus Status
        {
            get => Stage switch
            {
                MissionStage.InProgress or MissionStage.DataEntry or MissionStage.UnderReview or MissionStage.SubmittedForReview => MissionStatus.InProgress,
                MissionStage.Completed => MissionStatus.Completed,
                MissionStage.Cancelled or MissionStage.Rejected => MissionStatus.Cancelled,
                _ => MissionStatus.Planned
            };
            set => Stage = value switch
            {
                MissionStatus.InProgress => MissionStage.InProgress,
                MissionStatus.Completed => MissionStage.Completed,
                MissionStatus.Cancelled => MissionStage.Cancelled,
                _ => MissionStage.Created
            };
        }

        [NotMapped]
        public DateTime ScheduledDate
        {
            get => MissionDate;
            set => MissionDate = value;
        }

        [NotMapped]
        public User? AssignedTo
        {
            get => AssignedToUser;
            set => AssignedToUser = value;
        }

        [NotMapped]
        public User? Supervisor
        {
            get => ReviewerUser;
            set => ReviewerUser = value;
        }

        [NotMapped]
        public decimal? CheckInLatitude
        {
            get => CheckinLat;
            set => CheckinLat = value;
        }

        [NotMapped]
        public decimal? CheckInLongitude
        {
            get => CheckinLng;
            set => CheckinLng = value;
        }

        [NotMapped]
        public int CompletedPropertyCount
        {
            get => EnteredPropertyCount;
            set => EnteredPropertyCount = value;
        }
    }

    public class MissionStageHistory
    {
        public long Id { get; set; }
        public int MissionId { get; set; }
        public MissionStage? FromStage { get; set; }
        public MissionStage ToStage { get; set; }
        public int ChangedById { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string? TriggerAction { get; set; }

        public virtual InspectionMission Mission { get; set; } = null!;
        public virtual User ChangedBy { get; set; } = null!;
    }

    public class MissionPropertyEntry
    {
        public long Id { get; set; }
        public int MissionId { get; set; }
        public int? PropertyId { get; set; }
        public string? LocalId { get; set; }
        public int EnteredByUserId { get; set; }
        public DateTime EntryStartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EntryCompletedAt { get; set; }
        public decimal? DqsAtEntry { get; set; }
        public EntryStatus EntryStatus { get; set; } = EntryStatus.InProgress;
        public string? ReviewNotes { get; set; }
        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public virtual InspectionMission Mission { get; set; } = null!;
        public virtual Property? Property { get; set; }
        public virtual User EnteredBy { get; set; } = null!;
        public virtual User? ReviewedBy { get; set; }
    }

    public class MissionChecklistTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public MissionType? MissionType { get; set; }
        public string Items { get; set; } = "[]";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedById { get; set; }

        public virtual User CreatedBy { get; set; } = null!;
    }

    public class MissionChecklistResult
    {
        public long Id { get; set; }
        public int MissionId { get; set; }
        public int TemplateId { get; set; }
        public int CompletedByUserId { get; set; }
        public string Results { get; set; } = "[]";
        public decimal CompletionPercent { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public virtual InspectionMission Mission { get; set; } = null!;
        public virtual MissionChecklistTemplate Template { get; set; } = null!;
        public virtual User CompletedBy { get; set; } = null!;
    }
}
