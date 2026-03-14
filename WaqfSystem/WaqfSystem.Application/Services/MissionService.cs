using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class MissionService : IMissionService
    {
        private static string GetStageColor(MissionStage stage) => stage switch
        {
            MissionStage.Created => "#6b7280",
            MissionStage.Assigned => "#378ADD",
            MissionStage.Accepted => "#1D9E75",
            MissionStage.InProgress => "#D97706",
            MissionStage.DataEntry => "#6D28D9",
            MissionStage.SubmittedForReview => "#EA580C",
            MissionStage.UnderReview => "#C2410C",
            MissionStage.Completed => "#15803D",
            MissionStage.SentForCorrection => "#B91C1C",
            MissionStage.Cancelled => "#4B5563",
            MissionStage.Rejected => "#7F1D1D",
            _ => "#6b7280"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notifications;
        private readonly IGeographicService _geo;
        private readonly ILogger<MissionService> _logger;

        public MissionService(IUnitOfWork unitOfWork, INotificationService notifications, IGeographicService geo, ILogger<MissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _notifications = notifications;
            _geo = geo;
            _logger = logger;
        }

        public string GenerateMissionCode()
        {
            var year = DateTime.UtcNow.Year;
            var maxId = _unitOfWork.GetQueryable<InspectionMission>()
                .Where(x => x.CreatedAt.Year == year)
                .Select(x => (long?)x.Id)
                .Max() ?? 0;
            return $"MSN-{year}-{(maxId + 1):00000}";
        }

        public async Task<int> CreateAsync(CreateMissionDto dto, int userId)
        {
            var mission = new InspectionMission
            {
                MissionCode = GenerateMissionCode(),
                Title = dto.Title.Trim(),
                Description = dto.Description,
                MissionType = dto.MissionType,
                Stage = MissionStage.Created,
                Priority = dto.Priority,
                GovernorateId = dto.GovernorateId,
                DistrictId = dto.DistrictId,
                SubDistrictId = dto.SubDistrictId,
                TargetArea = dto.TargetArea,
                TargetPropertyCount = dto.TargetPropertyCount,
                MissionDate = dto.MissionDate.Date,
                ExpectedCompletionDate = dto.ExpectedCompletionDate?.Date,
                IsUrgent = dto.IsUrgent,
                AssignmentNotes = dto.AssignmentNotes,
                ReviewerUserId = dto.ReviewerUserId,
                ChecklistTemplateId = dto.ChecklistTemplateId,
                CurrentStageChangedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _unitOfWork.AddAsync(mission);
            await _unitOfWork.SaveChangesAsync();

            if (dto.PropertyIds is { Count: > 0 })
            {
                foreach (var propertyId in dto.PropertyIds.Distinct())
                {
                    await _unitOfWork.AddAsync(new MissionPropertyEntry
                    {
                        MissionId = mission.Id,
                        PropertyId = (int)propertyId,
                        EnteredByUserId = userId,
                        EntryStartedAt = DateTime.UtcNow,
                        EntryStatus = EntryStatus.InProgress
                    });
                }
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.AddAsync(new MissionStageHistory
            {
                MissionId = mission.Id,
                FromStage = null,
                ToStage = MissionStage.Created,
                ChangedById = userId,
                TriggerAction = "إنشاء المهمة",
                ChangedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            if (dto.AssignedToUserId.HasValue)
            {
                await AssignAsync(new AssignMissionDto
                {
                    MissionId = mission.Id,
                    AssignedToUserId = dto.AssignedToUserId.Value,
                    AssignedToTeamId = dto.AssignedToTeamId,
                    AssignmentNotes = dto.AssignmentNotes,
                    ExpectedCompletionDate = dto.ExpectedCompletionDate,
                    ReviewerUserId = dto.ReviewerUserId
                }, userId);
            }

            return mission.Id;
        }

        public async Task AssignAsync(AssignMissionDto dto, int userId)
        {
            var mission = await FindMissionAsync(dto.MissionId);
            if (mission == null) throw new ValidationException("المهمة غير موجودة");
            if (mission.Stage is MissionStage.Cancelled or MissionStage.Completed) throw new ValidationException("لا يمكن تكليف مهمة مكتملة أو ملغاة");

            await EnsureRoleAsync(userId, "SYS_ADMIN", "AUTH_DIRECTOR", "REGIONAL_MGR");
            await EnsureUserAssignableAsync(dto.AssignedToUserId);

            var previous = mission.Stage;
            mission.AssignedToUserId = dto.AssignedToUserId;
            mission.AssignedToTeamId = dto.AssignedToTeamId;
            mission.AssignedByUserId = userId;
            mission.AssignedAt = DateTime.UtcNow;
            mission.Stage = MissionStage.Assigned;
            mission.CurrentStageChangedAt = DateTime.UtcNow;
            mission.AssignmentNotes = dto.AssignmentNotes;
            mission.ExpectedCompletionDate = dto.ExpectedCompletionDate?.Date ?? mission.ExpectedCompletionDate;
            mission.ReviewerUserId = dto.ReviewerUserId ?? mission.ReviewerUserId;
            mission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.AddAsync(new MissionStageHistory
            {
                MissionId = mission.Id,
                FromStage = previous,
                ToStage = MissionStage.Assigned,
                ChangedById = userId,
                TriggerAction = "تكليف المهمة",
                Notes = dto.AssignmentNotes,
                ChangedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            await CreateUserNotificationAsync(dto.AssignedToUserId, "تكليف مهمة", $"تم تكليفك بمهمة جديدة: {mission.Title} — المنطقة: {mission.TargetArea ?? "غير محددة"}", (int)mission.Id);
        }

        public async Task ReassignAsync(ReassignMissionDto dto, int userId)
        {
            await EnsureRoleAsync(userId, "SYS_ADMIN", "AUTH_DIRECTOR", "REGIONAL_MGR");
            var mission = await FindMissionAsync(dto.MissionId);
            if (mission == null) throw new ValidationException("المهمة غير موجودة");
            await EnsureUserAssignableAsync(dto.NewUserId);

            var oldAssigneeId = mission.AssignedToUserId;
            var previous = mission.Stage;
            mission.AssignedToUserId = dto.NewUserId;
            mission.AssignedByUserId = userId;
            mission.AssignedAt = DateTime.UtcNow;
            mission.Stage = MissionStage.Assigned;
            mission.CurrentStageChangedAt = DateTime.UtcNow;
            mission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.AddAsync(new MissionStageHistory
            {
                MissionId = mission.Id,
                FromStage = previous,
                ToStage = MissionStage.Assigned,
                ChangedById = userId,
                TriggerAction = "إعادة إسناد",
                Notes = dto.ReassignReason,
                ChangedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            if (oldAssigneeId.HasValue)
            {
                await CreateUserNotificationAsync(oldAssigneeId.Value, "إعادة إسناد مهمة", $"تم إعادة إسناد المهمة {mission.MissionCode} إلى موظف آخر", (int)mission.Id);
            }

            await CreateUserNotificationAsync(dto.NewUserId, "تكليف مهمة", $"تم تكليفك بمهمة: {mission.Title}", (int)mission.Id);
        }

        public async Task<bool> AdvanceStageAsync(AdvanceStageDto dto, int userId)
        {
            var mission = await FindMissionAsync(dto.MissionId);
            if (mission == null) throw new ValidationException("المهمة غير موجودة");

            var user = await _unitOfWork.GetQueryable<User>().Include(x => x.Role).FirstAsync(x => x.Id == userId);
            ValidateTransition(mission.Stage, dto.ToStage);
            await AuthorizeTransitionAsync(mission, userId, user.Role.Code, dto.ToStage);

            var previous = mission.Stage;
            var now = DateTime.UtcNow;

            if (dto.ToStage == MissionStage.Accepted)
            {
                mission.AcceptedAt = now;
            }
            else if (dto.ToStage == MissionStage.InProgress)
            {
                if (!dto.CheckinLat.HasValue || !dto.CheckinLng.HasValue) throw new ValidationException("يجب إدخال الإحداثيات قبل بدء التنفيذ");
                mission.CheckinLat = dto.CheckinLat;
                mission.CheckinLng = dto.CheckinLng;
                mission.CheckinAt = now;
            }
            else if (dto.ToStage == MissionStage.SubmittedForReview)
            {
                var anyEntry = await _unitOfWork.GetQueryable<MissionPropertyEntry>()
                    .AnyAsync(x => x.MissionId == mission.Id && (x.DqsAtEntry ?? 0m) >= 50m);
                if (!anyEntry) throw new ValidationException("يجب إدخال عقار واحد على الأقل بدرجة جودة لا تقل عن 50%");
                mission.SubmittedAt = now;
            }
            else if (dto.ToStage == MissionStage.Completed)
            {
                mission.CompletedAt = now;
                mission.ActualCompletionDate = now.Date;
                await RecalculateMissionStatsAsync(mission.Id);
            }
            else if (dto.ToStage == MissionStage.SentForCorrection)
            {
                mission.CorrectionNotes = dto.Notes;
            }
            else if (dto.ToStage == MissionStage.Cancelled)
            {
                if (string.IsNullOrWhiteSpace(dto.Notes)) throw new ValidationException("يجب إدخال سبب الإلغاء");
                mission.CancellationReason = dto.Notes;
            }
            else if (dto.ToStage == MissionStage.Rejected)
            {
                if (string.IsNullOrWhiteSpace(dto.Notes)) throw new ValidationException("يجب إدخال سبب الرفض");
                mission.RejectionReason = dto.Notes;
            }

            mission.Stage = dto.ToStage;
            mission.CurrentStageChangedAt = now;
            mission.UpdatedAt = now;

            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.AddAsync(new MissionStageHistory
            {
                MissionId = mission.Id,
                FromStage = previous,
                ToStage = dto.ToStage,
                ChangedById = userId,
                Notes = dto.Notes,
                TriggerAction = GetTriggerAction(dto.ToStage),
                ChangedAt = now
            });
            await _unitOfWork.SaveChangesAsync();

            await NotifyByStageAsync(mission, user, dto.ToStage, dto.Notes);
            return true;
        }

        public async Task RecordPropertyEntryAsync(int missionId, long? propertyId, string? localId, int userId)
        {
            var mission = await FindMissionAsync(missionId);
            if (mission == null) throw new ValidationException("المهمة غير موجودة");
            if (mission.Stage is not (MissionStage.InProgress or MissionStage.DataEntry)) throw new ValidationException("لا يمكن إدخال عقار في هذه المرحلة");
            if (mission.AssignedToUserId != userId) throw new ValidationException("غير مصرح لك بإدخال بيانات هذه المهمة");

            if (propertyId.HasValue)
            {
                var duplicateProperty = await _unitOfWork.GetQueryable<MissionPropertyEntry>()
                    .AnyAsync(x => x.MissionId == missionId && x.PropertyId == (int)propertyId.Value);
                if (duplicateProperty) throw new ValidationException("هذا العقار مسجل ضمن المهمة مسبقًا");
            }

            if (!string.IsNullOrWhiteSpace(localId))
            {
                var duplicateLocal = await _unitOfWork.GetQueryable<MissionPropertyEntry>()
                    .AnyAsync(x => x.MissionId == missionId && x.LocalId == localId);
                if (duplicateLocal) throw new ValidationException("المعرف المحلي مستخدم مسبقًا ضمن المهمة");
            }

            await _unitOfWork.AddAsync(new MissionPropertyEntry
            {
                MissionId = missionId,
                PropertyId = propertyId.HasValue ? (int?)propertyId.Value : null,
                LocalId = localId,
                EnteredByUserId = userId,
                EntryStartedAt = DateTime.UtcNow,
                EntryStatus = EntryStatus.Submitted
            });

            mission.EnteredPropertyCount += 1;
            mission.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.SaveChangesAsync();

            await RecalculateMissionStatsAsync(missionId);
            if (mission.Stage == MissionStage.InProgress)
            {
                await AdvanceStageAsync(new AdvanceStageDto { MissionId = missionId, ToStage = MissionStage.DataEntry, Notes = "الانتقال التلقائي بعد إدخال أول عقار" }, userId);
            }
        }

        public async Task ApproveEntryAsync(long entryId, int userId)
        {
            var entry = await _unitOfWork.GetQueryable<MissionPropertyEntry>().Include(x => x.Mission).FirstOrDefaultAsync(x => x.Id == entryId);
            if (entry == null) throw new ValidationException("إدخال العقار غير موجود");

            entry.EntryStatus = EntryStatus.Approved;
            entry.ReviewedByUserId = userId;
            entry.ReviewedAt = DateTime.UtcNow;
            entry.Mission.ReviewedPropertyCount += 1;
            entry.Mission.ApprovedPropertyCount += 1;
            entry.Mission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(entry);
            await _unitOfWork.UpdateAsync(entry.Mission);
            await _unitOfWork.SaveChangesAsync();
            await RecalculateMissionStatsAsync(entry.MissionId);
        }

        public async Task RejectEntryAsync(long entryId, string notes, int userId)
        {
            var entry = await _unitOfWork.GetQueryable<MissionPropertyEntry>().Include(x => x.Mission).FirstOrDefaultAsync(x => x.Id == entryId);
            if (entry == null) throw new ValidationException("إدخال العقار غير موجود");

            entry.EntryStatus = EntryStatus.Rejected;
            entry.ReviewNotes = notes;
            entry.ReviewedByUserId = userId;
            entry.ReviewedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            if (entry.Mission.AssignedToUserId.HasValue)
            {
                await CreateUserNotificationAsync(entry.Mission.AssignedToUserId.Value, "رفض إدخال عقار", $"تم رفض إدخال عقار في المهمة {entry.Mission.MissionCode}: {notes}", (int)entry.MissionId);
            }
        }

        public async Task<PagedResult<MissionListItemDto>> GetPagedAsync(MissionFilterRequest filter, int userId, string userRole)
        {
            var scoped = await ScopedMissionsQuery(userId, userRole);

            if (filter.GovernorateId.HasValue) scoped = scoped.Where(x => x.GovernorateId == filter.GovernorateId.Value);
            if (filter.Stage.HasValue) scoped = scoped.Where(x => x.Stage == filter.Stage.Value);
            if (filter.MissionType.HasValue) scoped = scoped.Where(x => x.MissionType == filter.MissionType.Value);
            if (filter.AssignedToUserId.HasValue) scoped = scoped.Where(x => x.AssignedToUserId == filter.AssignedToUserId.Value);
            if (filter.Priority.HasValue) scoped = scoped.Where(x => x.Priority == filter.Priority.Value);
            if (filter.DateFrom.HasValue) scoped = scoped.Where(x => x.MissionDate >= filter.DateFrom.Value.Date);
            if (filter.DateTo.HasValue) scoped = scoped.Where(x => x.MissionDate <= filter.DateTo.Value.Date);
            if (filter.IsUrgent.HasValue) scoped = scoped.Where(x => x.IsUrgent == filter.IsUrgent.Value);
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                scoped = scoped.Where(x => x.MissionCode.Contains(term) || x.Title.Contains(term) || (x.TargetArea ?? string.Empty).Contains(term));
            }
            if (filter.IsOverdue == true)
            {
                scoped = scoped.Where(x => x.ExpectedCompletionDate.HasValue && x.ExpectedCompletionDate.Value < DateTime.Today && x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled);
            }

            scoped = ApplySorting(scoped, filter.SortBy, filter.SortDesc);
            var total = await scoped.CountAsync();

            var items = await scoped.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(x => new MissionListItemDto
                {
                    Id = x.Id,
                    MissionCode = x.MissionCode,
                    Title = x.Title,
                    MissionType = x.MissionType,
                    Stage = x.Stage,
                    Priority = x.Priority,
                    GovernorateNameAr = x.Governorate.NameAr,
                    DistrictNameAr = x.District != null ? x.District.NameAr : null,
                    AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.FullNameAr : null,
                    AssignedToAvatar = GetInitials(x.AssignedToUser != null ? x.AssignedToUser.FullNameAr : null),
                    MissionDate = x.MissionDate,
                    ExpectedCompletionDate = x.ExpectedCompletionDate,
                    TargetPropertyCount = x.TargetPropertyCount,
                    EnteredPropertyCount = x.EnteredPropertyCount,
                    ProgressPercent = x.ProgressPercent,
                    AverageDqsScore = x.AverageDqsScore,
                    IsUrgent = x.IsUrgent,
                    IsOverdue = x.ExpectedCompletionDate.HasValue && x.ExpectedCompletionDate.Value < DateTime.Today && x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled,
                    DaysRemaining = x.ExpectedCompletionDate.HasValue ? (x.ExpectedCompletionDate.Value - DateTime.Today).Days : 0,
                }).ToListAsync();

            foreach (var item in items)
            {
                item.StageColor = GetStageColor(item.Stage);
                item.StageDisplayAr = GetStageAr(item.Stage);
            }

            return new PagedResult<MissionListItemDto> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
        }

        public async Task<MissionDetailDto?> GetDetailAsync(int id, int userId, string userRole)
        {
            var mission = await (await ScopedMissionsQuery(userId, userRole))
                .Include(x => x.StageHistory).ThenInclude(x => x.ChangedBy)
                .Include(x => x.PropertyEntries).ThenInclude(x => x.EnteredBy)
                .Include(x => x.PropertyEntries).ThenInclude(x => x.ReviewedBy)
                .Include(x => x.PropertyEntries).ThenInclude(x => x.Property)
                .Include(x => x.ChecklistResults).ThenInclude(x => x.CompletedBy)
                .Include(x => x.ChecklistTemplate)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mission == null) return null;

            var detail = new MissionDetailDto
            {
                Id = mission.Id,
                MissionCode = mission.MissionCode,
                Title = mission.Title,
                Description = mission.Description,
                TargetArea = mission.TargetArea,
                MissionType = mission.MissionType,
                Stage = mission.Stage,
                Priority = mission.Priority,
                GovernorateNameAr = mission.Governorate.NameAr,
                DistrictNameAr = mission.District?.NameAr,
                AssignedToName = mission.AssignedToUser?.FullNameAr,
                AssignedToAvatar = GetInitials(mission.AssignedToUser?.FullNameAr),
                MissionDate = mission.MissionDate,
                ExpectedCompletionDate = mission.ExpectedCompletionDate,
                TargetPropertyCount = mission.TargetPropertyCount,
                EnteredPropertyCount = mission.EnteredPropertyCount,
                ProgressPercent = mission.ProgressPercent,
                AverageDqsScore = mission.AverageDqsScore,
                IsUrgent = mission.IsUrgent,
                IsOverdue = mission.IsOverdue,
                StageColor = GetStageColor(mission.Stage),
                StageDisplayAr = GetStageAr(mission.Stage),
                DaysRemaining = mission.ExpectedCompletionDate.HasValue ? (mission.ExpectedCompletionDate.Value - DateTime.Today).Days : 0,
                AssignedToUser = mission.AssignedToUser == null ? null : ToUserBrief(mission.AssignedToUser),
                AssignedByUser = mission.AssignedByUser == null ? null : ToUserBrief(mission.AssignedByUser),
                ReviewerUser = mission.ReviewerUser == null ? null : ToUserBrief(mission.ReviewerUser),
                Team = mission.AssignedToTeam == null ? null : new TeamBriefDto { Id = mission.AssignedToTeam.Id, TeamName = mission.AssignedToTeam.TeamName, TeamCode = mission.AssignedToTeam.TeamCode, MemberCount = mission.AssignedToTeam.Members.Count },
                StageHistory = mission.StageHistory.OrderByDescending(x => x.ChangedAt).Select(x => new MissionStageHistoryDto
                {
                    FromStage = x.FromStage,
                    ToStage = x.ToStage,
                    FromStageAr = x.FromStage.HasValue ? GetStageAr(x.FromStage.Value) : "بدء",
                    ToStageAr = GetStageAr(x.ToStage),
                    ChangedByName = x.ChangedBy.FullNameAr,
                    ChangedAt = x.ChangedAt,
                    Notes = x.Notes,
                    TriggerAction = x.TriggerAction
                }).ToList(),
                PropertyEntries = mission.PropertyEntries.OrderByDescending(x => x.EntryStartedAt).Select(x => new MissionPropertyEntryDto
                {
                    Id = x.Id,
                    PropertyId = x.PropertyId,
                    LocalId = x.LocalId,
                    PropertyNameAr = x.Property?.PropertyName,
                    PropertyWqfNumber = x.Property?.WqfNumber,
                    EnteredByName = x.EnteredBy.FullNameAr,
                    EntryStatus = x.EntryStatus,
                    DqsAtEntry = x.DqsAtEntry,
                    EntryStartedAt = x.EntryStartedAt,
                    EntryCompletedAt = x.EntryCompletedAt,
                    ReviewNotes = x.ReviewNotes,
                    ReviewedByName = x.ReviewedBy != null ? x.ReviewedBy.FullNameAr : null
                }).ToList(),
                ChecklistTemplate = mission.ChecklistTemplate == null ? null : new ChecklistTemplateDto { Id = mission.ChecklistTemplate.Id, TemplateName = mission.ChecklistTemplate.TemplateName, Items = ParseChecklistItems(mission.ChecklistTemplate.Items) },
                ChecklistResults = mission.ChecklistResults.Select(x => new ChecklistResultDto { TemplateId = x.TemplateId, CompletionPercent = x.CompletionPercent, Results = ParseChecklistResults(x.Results), CompletedByName = x.CompletedBy.FullNameAr }).ToList(),
                ProgressStats = BuildProgressStats(mission)
            };

            detail.AllowedNextStages = GetAllowedTransitions(mission.Stage).Where(next => IsTransitionAllowedByRole(next, userRole, mission, userId)).ToList();
            detail.CanAccept = mission.Stage == MissionStage.Assigned && mission.AssignedToUserId == userId;
            detail.CanReject = detail.CanAccept;
            detail.CanCheckin = mission.Stage == MissionStage.Accepted && mission.AssignedToUserId == userId;
            detail.CanSubmitReview = mission.Stage == MissionStage.DataEntry && mission.AssignedToUserId == userId;
            detail.CanApprove = mission.Stage == MissionStage.UnderReview && (userRole == "FIELD_SUPERVISOR" || IsAdminRole(userRole));
            detail.CanSendBack = detail.CanApprove;
            detail.CanCancel = IsAdminRole(userRole);
            detail.CanReassign = IsAdminRole(userRole);

            return detail;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string userRole)
        {
            var scoped = await ScopedMissionsQuery(userId, userRole);
            var now = DateTime.Today;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var list = await scoped.ToListAsync();

            var stats = new DashboardStatsDto
            {
                TotalMissions = list.Count,
                ActiveMissions = list.Count(x => x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled),
                CompletedThisMonth = list.Count(x => x.Stage == MissionStage.Completed && x.CompletedAt.HasValue && x.CompletedAt.Value.Date >= monthStart && x.CompletedAt.Value.Date <= monthEnd),
                OverdueMissions = list.Count(x => x.ExpectedCompletionDate.HasValue && x.ExpectedCompletionDate.Value.Date < now && x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled),
                TotalEmployees = await _unitOfWork.GetQueryable<User>().CountAsync(x => !x.IsDeleted && x.IsActive && x.Role.Code == "FIELD_INSPECTOR"),
                ActiveEmployees = await _unitOfWork.GetQueryable<User>().CountAsync(x => !x.IsDeleted && x.IsActive && x.Role.Code == "FIELD_INSPECTOR" && _unitOfWork.GetQueryable<InspectionMission>().Any(m => m.AssignedToUserId == x.Id && m.Stage != MissionStage.Completed && m.Stage != MissionStage.Cancelled)),
                AverageDqsScore = list.Any() ? Math.Round(list.Where(x => x.AverageDqsScore.HasValue).DefaultIfEmpty().Average(x => x?.AverageDqsScore ?? 0), 2) : 0,
                TotalPropertiesEnteredThisMonth = list.Where(x => x.CreatedAt.Date >= monthStart && x.CreatedAt.Date <= monthEnd).Sum(x => x.EnteredPropertyCount)
            };

            stats.MissionsByStage = Enum.GetValues<MissionStage>().ToDictionary(x => x.ToString(), x => list.Count(m => m.Stage == x));
            stats.MissionsByType = Enum.GetValues<MissionType>().ToDictionary(x => x.ToString(), x => list.Count(m => m.MissionType == x));
            stats.TopPerformers = (await GetEmployeePerformanceAsync(null, monthStart, monthEnd, userId, userRole)).Take(5).ToList();

            stats.RecentActivity = await _unitOfWork.GetQueryable<MissionStageHistory>()
                .AsNoTracking()
                .Include(x => x.ChangedBy)
                .OrderByDescending(x => x.ChangedAt)
                .Take(10)
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => new MissionStageHistoryDto
                {
                    FromStage = x.FromStage,
                    ToStage = x.ToStage,
                    FromStageAr = x.FromStage.HasValue ? GetStageAr(x.FromStage.Value) : "بدء",
                    ToStageAr = GetStageAr(x.ToStage),
                    ChangedByName = x.ChangedBy.FullNameAr,
                    ChangedAt = x.ChangedAt,
                    Notes = x.Notes,
                    TriggerAction = x.TriggerAction
                }).ToList());

            var upcomingMissions = await scoped.Where(x => x.MissionDate >= now && x.MissionDate <= now.AddDays(7)).OrderBy(x => x.MissionDate).Take(7)
                .Select(x => new MissionListItemDto
                {
                    Id = x.Id,
                    MissionCode = x.MissionCode,
                    Title = x.Title,
                    MissionType = x.MissionType,
                    Stage = x.Stage,
                    Priority = x.Priority,
                    GovernorateNameAr = x.Governorate.NameAr,
                    AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.FullNameAr : string.Empty,
                    MissionDate = x.MissionDate,
                    ProgressPercent = x.ProgressPercent,
                }).ToListAsync();

            foreach (var m in upcomingMissions)
            {
                m.StageColor = GetStageColor(m.Stage);
                m.StageDisplayAr = GetStageAr(m.Stage);
            }
            stats.UpcomingMissions = upcomingMissions;

            return stats;
        }

        public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(int year, int month, int userId, string userRole)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var events = await (await ScopedMissionsQuery(userId, userRole))
                .Where(x => x.MissionDate >= start && x.MissionDate <= end)
                .Select(x => new CalendarEventDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    MissionType = x.MissionType,
                    Stage = x.Stage,
                    Priority = x.Priority,
                    Start = x.MissionDate,
                    End = x.ExpectedCompletionDate,
                    AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.FullNameAr : "غير محدد",
                    GovernorateNameAr = x.Governorate.NameAr,
                    IsUrgent = x.IsUrgent
                })
                .ToListAsync();

            foreach (var ev in events)
            {
                ev.Color = GetStageColor(ev.Stage);
            }

            return events;
        }

        public async Task<List<EmployeePerformanceDto>> GetEmployeePerformanceAsync(int? governorateId, DateTime from, DateTime to, int userId, string userRole)
        {
            var users = _unitOfWork.GetQueryable<User>().AsNoTracking().Include(x => x.Role).Include(x => x.Governorate)
                .Where(x => !x.IsDeleted && x.IsActive && x.Role.Code == "FIELD_INSPECTOR");

            if (governorateId.HasValue) users = users.Where(x => x.GovernorateId == governorateId.Value);
            if (userRole == "REGIONAL_MGR")
            {
                var myGov = await _unitOfWork.GetQueryable<User>().Where(x => x.Id == userId).Select(x => x.GovernorateId).FirstOrDefaultAsync();
                users = users.Where(x => x.GovernorateId == myGov);
            }

            var userList = await users.ToListAsync();
            var result = new List<EmployeePerformanceDto>();
            foreach (var u in userList)
            {
                var missions = await _unitOfWork.GetQueryable<InspectionMission>()
                    .AsNoTracking()
                    .Where(x => x.AssignedToUserId == u.Id && x.MissionDate >= from.Date && x.MissionDate <= to.Date)
                    .ToListAsync();

                var completed = missions.Where(x => x.Stage == MissionStage.Completed).ToList();
                var avgDays = completed.Any() ? (decimal)completed.Where(x => x.CompletedAt.HasValue).Select(x => (x.CompletedAt!.Value.Date - x.MissionDate.Date).TotalDays).DefaultIfEmpty().Average() : 0m;
                var onTimeCount = completed.Count(x => x.ExpectedCompletionDate.HasValue && x.CompletedAt.HasValue && x.CompletedAt.Value.Date <= x.ExpectedCompletionDate.Value.Date);
                var onTimeRate = completed.Count == 0 ? 0m : Math.Round((decimal)onTimeCount / completed.Count * 100m, 2);
                var avgDqs = missions.Where(x => x.AverageDqsScore.HasValue).Select(x => x.AverageDqsScore!.Value).DefaultIfEmpty(0).Average();

                result.Add(new EmployeePerformanceDto
                {
                    UserId = u.Id,
                    FullName = u.FullNameAr,
                    Role = "موظف ميداني",
                    GovernorateNameAr = u.Governorate?.NameAr ?? "-",
                    TotalMissionsAssigned = missions.Count,
                    CompletedMissions = completed.Count,
                    InProgressMissions = missions.Count(x => x.Stage is MissionStage.Assigned or MissionStage.Accepted or MissionStage.InProgress or MissionStage.DataEntry),
                    AverageDqsScore = Math.Round(avgDqs, 2),
                    AverageCompletionDays = Math.Round(avgDays, 2),
                    OnTimeCompletionRate = onTimeRate,
                    TotalPropertiesEntered = missions.Sum(x => x.EnteredPropertyCount),
                    ApprovedPropertiesCount = missions.Sum(x => x.ApprovedPropertyCount),
                    LastMissionDate = missions.OrderByDescending(x => x.MissionDate).Select(x => (DateTime?)x.MissionDate).FirstOrDefault(),
                    PerformanceRating = GetPerformanceRate(onTimeRate, avgDqs)
                });
            }

            return result.OrderByDescending(x => x.CompletedMissions).ThenByDescending(x => x.AverageDqsScore).ToList();
        }

        public async Task<TeamDetailDto?> GetTeamDetailAsync(int teamId)
        {
            var team = await _unitOfWork.GetQueryable<InspectionTeam>().AsNoTracking()
                .Include(x => x.Governorate)
                .Include(x => x.Leader)
                .Include(x => x.Members).ThenInclude(x => x.User).ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == teamId);
            if (team == null) return null;

            return new TeamDetailDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                TeamCode = team.TeamCode,
                GovernorateId = team.GovernorateId,
                GovernorateNameAr = team.Governorate.NameAr,
                LeaderId = team.LeaderId,
                LeaderName = team.Leader?.FullNameAr,
                Description = team.Description,
                IsActive = team.IsActive,
                CreatedAt = team.CreatedAt,
                ActiveMissionCount = await _unitOfWork.GetQueryable<InspectionMission>().CountAsync(x => x.AssignedToTeamId == teamId && x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled),
                Members = team.Members.Select(m => new TeamMemberDto
                {
                    UserId = m.UserId,
                    FullName = m.User.FullNameAr,
                    Role = m.User.Role.NameAr,
                    Phone = m.User.PhoneNumber,
                    JoinedAt = m.JoinedAt,
                    IsActive = m.IsActive,
                    ActiveMissionCount = _unitOfWork.GetQueryable<InspectionMission>().Count(x => x.AssignedToUserId == m.UserId && x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled)
                }).ToList()
            };
        }

        public async Task<List<TeamDetailDto>> GetTeamsAsync(int? governorateId)
        {
            IQueryable<InspectionTeam> query = _unitOfWork.GetQueryable<InspectionTeam>()
                .AsNoTracking()
                .Include(x => x.Governorate)
                .Include(x => x.Members)
                .AsQueryable();
            if (governorateId.HasValue) query = query.Where(x => x.GovernorateId == governorateId.Value);
            var teams = await query.OrderBy(x => x.TeamName).ToListAsync();

            return teams.Select(x => new TeamDetailDto
            {
                Id = x.Id,
                TeamName = x.TeamName,
                TeamCode = x.TeamCode,
                GovernorateId = x.GovernorateId,
                GovernorateNameAr = x.Governorate.NameAr,
                LeaderId = x.LeaderId,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                ActiveMissionCount = _unitOfWork.GetQueryable<InspectionMission>().Count(m => m.AssignedToTeamId == x.Id && m.Stage != MissionStage.Completed && m.Stage != MissionStage.Cancelled),
                Members = new List<TeamMemberDto>()
            }).ToList();
        }

        public async Task<int> CreateTeamAsync(TeamCreateDto dto, int userId)
        {
            var team = new InspectionTeam
            {
                TeamName = dto.TeamName,
                TeamCode = dto.TeamCode,
                GovernorateId = dto.GovernorateId,
                LeaderId = dto.LeaderId,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };
            await _unitOfWork.AddAsync(team);
            await _unitOfWork.SaveChangesAsync();
            return team.Id;
        }

        public async Task AddTeamMemberAsync(int teamId, int userId, int addedById)
        {
            var exists = await _unitOfWork.GetQueryable<InspectionTeamMember>().AnyAsync(x => x.TeamId == teamId && x.UserId == userId);
            if (exists) return;

            await _unitOfWork.AddAsync(new InspectionTeamMember { TeamId = teamId, UserId = userId, AddedById = addedById, IsActive = true, JoinedAt = DateTime.UtcNow });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveTeamMemberAsync(int teamId, int userId, int removedById)
        {
            var member = await _unitOfWork.GetQueryable<InspectionTeamMember>().FirstOrDefaultAsync(x => x.TeamId == teamId && x.UserId == userId);
            if (member == null) return;
            await _unitOfWork.DeleteAsync(member);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<InspectionMission> CreateMissionAsync(string title, int? governorateId, int? supervisorId, int? assignedToId, MissionType type)
        {
            var id = await CreateAsync(new CreateMissionDto
            {
                Title = title,
                GovernorateId = governorateId ?? 0,
                MissionType = type,
                MissionDate = DateTime.Today,
                Priority = MissionPriority.Normal,
                AssignedToUserId = assignedToId,
                ReviewerUserId = supervisorId,
                TargetPropertyCount = 0
            }, 1);
            return await _unitOfWork.GetQueryable<InspectionMission>().FirstAsync(x => x.Id == id);
        }

        public async Task<List<InspectionMission>> GetMyMissionsAsync(int userId)
        {
            return await _unitOfWork.GetQueryable<InspectionMission>().AsNoTracking().Where(x => x.AssignedToUserId == userId).OrderByDescending(x => x.MissionDate).ToListAsync();
        }

        public async Task<bool> StartMissionAsync(int missionId, decimal lat, decimal lng)
        {
            return await AdvanceStageAsync(new AdvanceStageDto { MissionId = missionId, ToStage = MissionStage.InProgress, CheckinLat = lat, CheckinLng = lng, Notes = "تسجيل وصول" }, 1);
        }

        public async Task<bool> CompleteMissionAsync(int missionId, string notes)
        {
            return await AdvanceStageAsync(new AdvanceStageDto { MissionId = missionId, ToStage = MissionStage.Completed, Notes = notes }, 1);
        }

        public async Task<bool> UpdateProgressAsync(int missionId, int completedCount)
        {
            var mission = await FindMissionAsync(missionId);
            if (mission == null) return false;
            mission.EnteredPropertyCount = completedCount;
            mission.ProgressPercent = mission.TargetPropertyCount <= 0 ? 0 : Math.Round((decimal)completedCount / mission.TargetPropertyCount * 100m, 2);
            mission.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<IQueryable<InspectionMission>> ScopedMissionsQuery(int userId, string userRole)
        {
            var query = _unitOfWork.GetQueryable<InspectionMission>()
                .AsNoTracking()
                .Include(x => x.Governorate)
                .Include(x => x.District)
                .Include(x => x.AssignedToUser)
                .Include(x => x.AssignedToTeam)
                .ThenInclude(x => x!.Members)
                .AsQueryable();

            if (userRole == "FIELD_INSPECTOR")
            {
                query = query.Where(x => x.AssignedToUserId == userId);
            }
            else if (userRole == "FIELD_SUPERVISOR")
            {
                var memberTeamIds = await _unitOfWork.GetQueryable<InspectionTeamMember>().Where(x => x.UserId == userId && x.IsActive).Select(x => x.TeamId).ToListAsync();
                var teamUserIds = await _unitOfWork.GetQueryable<InspectionTeamMember>().Where(x => memberTeamIds.Contains(x.TeamId) && x.IsActive).Select(x => x.UserId).Distinct().ToListAsync();
                query = query.Where(x => (x.AssignedToUserId.HasValue && teamUserIds.Contains(x.AssignedToUserId.Value)) || x.ReviewerUserId == userId);
            }
            else if (userRole == "REGIONAL_MGR")
            {
                var myGov = await _unitOfWork.GetQueryable<User>().Where(x => x.Id == userId).Select(x => x.GovernorateId).FirstOrDefaultAsync();
                query = query.Where(x => x.GovernorateId == myGov);
            }

            return query;
        }

        private static IQueryable<InspectionMission> ApplySorting(IQueryable<InspectionMission> query, string sortBy, bool desc)
        {
            return (sortBy, desc) switch
            {
                ("Stage", true) => query.OrderByDescending(x => x.Stage),
                ("Stage", false) => query.OrderBy(x => x.Stage),
                ("Priority", true) => query.OrderByDescending(x => x.Priority),
                ("Priority", false) => query.OrderBy(x => x.Priority),
                ("ProgressPercent", true) => query.OrderByDescending(x => x.ProgressPercent),
                ("ProgressPercent", false) => query.OrderBy(x => x.ProgressPercent),
                ("MissionCode", true) => query.OrderByDescending(x => x.MissionCode),
                ("MissionCode", false) => query.OrderBy(x => x.MissionCode),
                ("MissionDate", true) => query.OrderByDescending(x => x.MissionDate),
                _ => query.OrderBy(x => x.MissionDate)
            };
        }

        private async Task RecalculateMissionStatsAsync(long missionId)
        {
            var mission = await FindMissionAsync(missionId) ?? throw new ValidationException("المهمة غير موجودة");
            var entries = await _unitOfWork.GetQueryable<MissionPropertyEntry>().Where(x => x.MissionId == missionId).ToListAsync();

            mission.EnteredPropertyCount = entries.Count;
            mission.ReviewedPropertyCount = entries.Count(x => x.EntryStatus is EntryStatus.Approved or EntryStatus.Rejected or EntryStatus.UnderReview);
            mission.ApprovedPropertyCount = entries.Count(x => x.EntryStatus == EntryStatus.Approved);
            mission.AverageDqsScore = entries.Where(x => x.DqsAtEntry.HasValue).Select(x => x.DqsAtEntry!.Value).DefaultIfEmpty(0).Average();
            mission.ProgressPercent = mission.TargetPropertyCount <= 0 ? 0 : Math.Round((decimal)mission.EnteredPropertyCount / mission.TargetPropertyCount * 100m, 2);
            mission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UpdateAsync(mission);
            await _unitOfWork.SaveChangesAsync();
        }

        private static string GetPerformanceRate(decimal onTimeRate, decimal avgDqs)
        {
            if (onTimeRate >= 90 && avgDqs >= 85) return "ممتاز";
            if (onTimeRate >= 75 && avgDqs >= 70) return "جيد";
            if (onTimeRate >= 55 && avgDqs >= 50) return "متوسط";
            return "يحتاج تحسين";
        }

        private static MissionProgressStatsDto BuildProgressStats(InspectionMission mission)
        {
            return new MissionProgressStatsDto
            {
                TargetCount = mission.TargetPropertyCount,
                EnteredCount = mission.EnteredPropertyCount,
                ReviewedCount = mission.ReviewedPropertyCount,
                ApprovedCount = mission.ApprovedPropertyCount,
                ProgressPercent = mission.ProgressPercent,
                AverageDqsScore = mission.AverageDqsScore ?? 0,
                Below50DqsCount = 0,
                Below70DqsCount = 0,
                IsOnSchedule = !mission.IsOverdue,
                DaysRemaining = mission.DaysRemaining,
                IsOverdue = mission.IsOverdue
            };
        }

        private async Task EnsureRoleAsync(int userId, params string[] allowed)
        {
            var role = await _unitOfWork.GetQueryable<User>().Where(x => x.Id == userId).Select(x => x.Role.Code).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(role) || !allowed.Contains(role)) throw new ValidationException("غير مصرح لك بتنفيذ هذا الإجراء");
        }

        private async Task EnsureUserAssignableAsync(int userId)
        {
            var role = await _unitOfWork.GetQueryable<User>().Where(x => x.Id == userId && !x.IsDeleted && x.IsActive).Select(x => x.Role.Code).FirstOrDefaultAsync();
            if (role is not ("FIELD_INSPECTOR" or "FIELD_SUPERVISOR")) throw new ValidationException("المستخدم المحدد غير مؤهل للإسناد");
        }

        private static void ValidateTransition(MissionStage from, MissionStage to)
        {
            var allowed = GetAllowedTransitions(from);
            if (!allowed.Contains(to) && to != MissionStage.Cancelled) throw new ValidationException($"الانتقال من {GetStageAr(from)} إلى {GetStageAr(to)} غير مسموح");
        }

        private static List<MissionStage> GetAllowedTransitions(MissionStage from)
        {
            return from switch
            {
                MissionStage.Created => new List<MissionStage> { MissionStage.Assigned },
                MissionStage.Assigned => new List<MissionStage> { MissionStage.Accepted, MissionStage.Rejected },
                MissionStage.Accepted => new List<MissionStage> { MissionStage.InProgress },
                MissionStage.InProgress => new List<MissionStage> { MissionStage.DataEntry },
                MissionStage.DataEntry => new List<MissionStage> { MissionStage.SubmittedForReview },
                MissionStage.SubmittedForReview => new List<MissionStage> { MissionStage.UnderReview },
                MissionStage.UnderReview => new List<MissionStage> { MissionStage.Completed, MissionStage.SentForCorrection },
                MissionStage.SentForCorrection => new List<MissionStage> { MissionStage.DataEntry },
                _ => new List<MissionStage>()
            };
        }

        private async Task AuthorizeTransitionAsync(InspectionMission mission, int userId, string role, MissionStage toStage)
        {
            if (toStage == MissionStage.Cancelled)
            {
                if (!IsAdminRole(role)) throw new ValidationException("فقط المدير يمكنه إلغاء المهمة");
                return;
            }

            if (toStage is MissionStage.Accepted or MissionStage.Rejected or MissionStage.InProgress or MissionStage.SubmittedForReview)
            {
                if (mission.AssignedToUserId != userId) throw new ValidationException("يسمح بهذا الإجراء للمكلّف بالمهمة فقط");
            }

            if (toStage is MissionStage.UnderReview or MissionStage.Completed or MissionStage.SentForCorrection)
            {
                if (!(role == "FIELD_SUPERVISOR" || IsAdminRole(role))) throw new ValidationException("يسمح بالمراجعة للمشرف أو المدير فقط");
            }

            await Task.CompletedTask;
        }

        private static bool IsTransitionAllowedByRole(MissionStage next, string userRole, InspectionMission mission, int userId)
        {
            if (next == MissionStage.Cancelled) return IsAdminRole(userRole);
            if (next is MissionStage.Accepted or MissionStage.Rejected or MissionStage.InProgress or MissionStage.SubmittedForReview) return mission.AssignedToUserId == userId;
            if (next is MissionStage.UnderReview or MissionStage.Completed or MissionStage.SentForCorrection) return userRole == "FIELD_SUPERVISOR" || IsAdminRole(userRole);
            return IsAdminRole(userRole) || userRole == "FIELD_SUPERVISOR";
        }

        private static bool IsAdminRole(string role) => role is "SYS_ADMIN" or "AUTH_DIRECTOR" or "REGIONAL_MGR";

        private static string GetTriggerAction(MissionStage stage)
        {
            return stage switch
            {
                MissionStage.Assigned => "تكليف المهمة",
                MissionStage.Accepted => "قبول المهمة",
                MissionStage.Rejected => "رفض المهمة",
                MissionStage.InProgress => "تسجيل وصول",
                MissionStage.DataEntry => "بدء إدخال البيانات",
                MissionStage.SubmittedForReview => "تقديم للمراجعة",
                MissionStage.UnderReview => "بدء المراجعة",
                MissionStage.Completed => "اعتماد المهمة",
                MissionStage.SentForCorrection => "إعادة للتصحيح",
                MissionStage.Cancelled => "إلغاء المهمة",
                _ => "تغيير حالة"
            };
        }

        private async Task NotifyByStageAsync(InspectionMission mission, User actor, MissionStage stage, string? notes)
        {
            if (stage == MissionStage.Accepted && mission.AssignedByUserId.HasValue)
            {
                await CreateUserNotificationAsync(mission.AssignedByUserId.Value, "قبول المهمة", $"قبل المفتش {actor.FullNameAr} المهمة {mission.MissionCode}", (int)mission.Id);
            }
            else if (stage == MissionStage.InProgress && mission.AssignedByUserId.HasValue)
            {
                await CreateUserNotificationAsync(mission.AssignedByUserId.Value, "بدء التنفيذ", $"بدأ المفتش تنفيذ المهمة {mission.MissionCode}", (int)mission.Id);
            }
            else if (stage == MissionStage.SubmittedForReview)
            {
                if (mission.ReviewerUserId.HasValue)
                {
                    await CreateUserNotificationAsync(mission.ReviewerUserId.Value, "جاهزة للمراجعة", $"المهمة {mission.MissionCode} جاهزة للمراجعة", (int)mission.Id);
                }
                else
                {
                    await _notifications.SendToRoleAsync("REGIONAL_MGR", "جاهزة للمراجعة", $"المهمة {mission.MissionCode} جاهزة للمراجعة", "InspectionMissions", (int)mission.Id);
                }
            }
            else if (stage == MissionStage.Completed && mission.AssignedToUserId.HasValue)
            {
                await CreateUserNotificationAsync(mission.AssignedToUserId.Value, "تم اعتماد المهمة", $"تم اعتماد المهمة {mission.MissionCode} بنجاح", (int)mission.Id);
            }
            else if (stage == MissionStage.SentForCorrection && mission.AssignedToUserId.HasValue)
            {
                await CreateUserNotificationAsync(mission.AssignedToUserId.Value, "تصحيح مطلوب", $"المهمة {mission.MissionCode} تحتاج تصحيحات: {notes}", (int)mission.Id);
            }
            else if (stage == MissionStage.Cancelled && mission.AssignedToUserId.HasValue)
            {
                await CreateUserNotificationAsync(mission.AssignedToUserId.Value, "إلغاء المهمة", $"تم إلغاء المهمة {mission.MissionCode}: {notes}", (int)mission.Id);
            }
        }

        private async Task CreateUserNotificationAsync(int userId, string title, string message, int missionId)
        {
            await _unitOfWork.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = NotificationType.MissionAssignment,
                ReferenceTable = "InspectionMissions",
                ReferenceId = missionId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1
            });
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<InspectionMission?> FindMissionAsync(long missionId)
        {
            return await _unitOfWork.GetQueryable<InspectionMission>().FirstOrDefaultAsync(x => x.Id == missionId);
        }

        private static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "--";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).ToArray();
            if (parts.Length == 1) return parts[0].Length > 0 ? parts[0][0].ToString() : "--";
            return string.Concat(parts[0][0], parts[1][0]).ToUpperInvariant();
        }

        private static string GetStageAr(MissionStage stage)
        {
            return stage switch
            {
                MissionStage.Created => "مُنشأة",
                MissionStage.Assigned => "مُكلَّفة",
                MissionStage.Accepted => "مقبولة",
                MissionStage.InProgress => "جارية",
                MissionStage.DataEntry => "إدخال البيانات",
                MissionStage.SubmittedForReview => "مُقدَّمة للمراجعة",
                MissionStage.UnderReview => "قيد المراجعة",
                MissionStage.Completed => "مكتملة",
                MissionStage.SentForCorrection => "مُعادة للتصحيح",
                MissionStage.Cancelled => "ملغاة",
                MissionStage.Rejected => "مرفوضة",
                _ => stage.ToString()
            };
        }

        private static UserBriefDto ToUserBrief(User u)
        {
            return new UserBriefDto
            {
                Id = u.Id,
                FullName = u.FullNameAr,
                Role = u.Role.NameAr,
                GovernorateId = u.GovernorateId,
                Phone = u.PhoneNumber,
                AvatarInitials = GetInitials(u.FullNameAr)
            };
        }

        private static List<ChecklistItemDefinitionDto> ParseChecklistItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<ChecklistItemDefinitionDto>();
            try
            {
                return JsonSerializer.Deserialize<List<ChecklistItemDefinitionDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChecklistItemDefinitionDto>();
            }
            catch
            {
                return new List<ChecklistItemDefinitionDto>();
            }
        }

        private static List<ChecklistItemResult> ParseChecklistResults(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<ChecklistItemResult>();
            try
            {
                return JsonSerializer.Deserialize<List<ChecklistItemResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChecklistItemResult>();
            }
            catch
            {
                return new List<ChecklistItemResult>();
            }
        }
    }
}
