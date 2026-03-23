using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;
using WaqfSystem.Web.Hubs;
using WaqfSystem.Web.ViewModels.Mission;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class MissionsController : Controller
    {
        private readonly IMissionService _missionService;
        private readonly IGeographicService _geographicService;
        private readonly ILogger<MissionsController> _logger;
        private readonly IHubContext<MissionHub> _hub;

        public MissionsController(IMissionService missionService, IGeographicService geographicService, ILogger<MissionsController> logger, IHubContext<MissionHub> hub)
        {
            _missionService = missionService;
            _geographicService = geographicService;
            _logger = logger;
            _hub = hub;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        private bool IsAdmin() => GetUserRole() is "SYS_ADMIN" or "AUTH_DIRECTOR" or "REGIONAL_MGR";
        private bool IsSupervisor() => GetUserRole() == "FIELD_SUPERVISOR";
        private bool IsInspector() => GetUserRole() == "FIELD_INSPECTOR";

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = new MissionDashboardViewModel
            {
                Stats = await _missionService.GetDashboardStatsAsync(GetUserId(), GetUserRole()),
                CurrentUserRole = GetUserRole(),
                CurrentUserName = User.Identity?.Name ?? "",
                GovernorateFilter = (await _geographicService.GetGovernoratesAsync())
                    .Select(x => new SelectListItem(x.NameAr, x.Id.ToString()))
                    .ToList()
            };

            return View("Dashboard", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Index(MissionFilterRequest filter)
        {
            var missions = await _missionService.GetPagedAsync(filter, GetUserId(), GetUserRole());
            var vm = new MissionIndexViewModel
            {
                Missions = missions,
                Filter = filter,
                Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                Employees = (await _missionService.GetEmployeePerformanceAsync(null, DateTime.Today.AddMonths(-1), DateTime.Today, GetUserId(), GetUserRole()))
                    .Select(x => new SelectListItem(x.FullName, x.UserId.ToString())).ToList(),
                CurrentUserRole = GetUserRole(),
                TotalActive = missions.Items.Count(x => x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled),
                TotalOverdue = missions.Items.Count(x => x.IsOverdue),
                TotalCompletedThisMonth = missions.Items.Count(x => x.Stage == MissionStage.Completed && x.MissionDate.Month == DateTime.Today.Month)
            };

            return View("Index", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Calendar(int? year, int? month, int? governorateId)
        {
            var y = year ?? DateTime.Today.Year;
            var m = month ?? DateTime.Today.Month;
            var events = await _missionService.GetCalendarEventsAsync(y, m, GetUserId(), GetUserRole());
            if (governorateId.HasValue)
            {
                var govs = await _geographicService.GetGovernoratesAsync();
                var govName = govs.FirstOrDefault(g => g.Id == governorateId.Value)?.NameAr ?? string.Empty;
                events = events.Where(x => x.GovernorateNameAr == govName).ToList();
            }

            var cur = new DateTime(y, m, 1);
            var prev = cur.AddMonths(-1);
            var next = cur.AddMonths(1);
            var vm = new MissionCalendarViewModel
            {
                Events = events,
                Year = y,
                Month = m,
                MonthNameAr = cur.ToString("MMMM", new CultureInfo("ar-IQ")),
                PrevYear = prev.Year,
                PrevMonth = prev.Month,
                NextYear = next.Year,
                NextMonth = next.Month,
                Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                FilterGovernorateId = governorateId
            };

            return View("Calendar", vm);
        }

        [HttpGet]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Create()
        {
            var vm = new MissionCreateViewModel
            {
                Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                Employees = (await _missionService.GetEmployeePerformanceAsync(null, DateTime.Today.AddMonths(-2), DateTime.Today, GetUserId(), GetUserRole())).Select(x => new SelectListItem(x.FullName, x.UserId.ToString())).ToList(),
                Teams = (await _missionService.GetTeamsAsync(null)).Select(x => new SelectListItem($"{x.TeamName} ({x.TeamCode})", x.Id.ToString())).ToList(),
                ChecklistTemplates = new() { new SelectListItem("بدون قائمة تفتيش", "") },
                MissionTypeOptions = Enum.GetValues<MissionType>().Select(x => new SelectListItem(x.ToString(), x.ToString())).ToList(),
                PriorityOptions = Enum.GetValues<MissionPriority>().Select(x => new SelectListItem(x.ToString(), x.ToString())).ToList()
            };
            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Create(CreateMissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return await Create();
            }

            var id = await _missionService.CreateAsync(dto, GetUserId());
            TempData["Success"] = $"تم إنشاء المهمة بنجاح — رقم: {id}";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var mission = await _missionService.GetDetailAsync(id, GetUserId(), GetUserRole());
            if (mission == null)
            {
                return NotFound();
            }

            return View("Detail", new MissionDetailViewModel
            {
                Mission = mission,
                CurrentUserId = GetUserId(),
                CurrentUserRole = GetUserRole()
            });
        }

        [HttpGet]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Edit(int id)
        {
            var mission = await _missionService.GetDetailAsync(id, GetUserId(), GetUserRole());
            if (mission == null)
            {
                return NotFound();
            }

            return View("Edit", new UpdateMissionDto
            {
                Id = id,
                Title = mission.Title,
                MissionType = mission.MissionType,
                MissionDate = mission.MissionDate,
                GovernorateId = (await _geographicService.GetGovernoratesAsync()).FirstOrDefault(g => g.NameAr == mission.GovernorateNameAr)?.Id,
                Priority = mission.Priority,
                ExpectedCompletionDate = mission.ExpectedCompletionDate,
                Description = mission.Description,
                TargetArea = mission.TargetArea,
                TargetPropertyCount = mission.TargetPropertyCount
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Edit(UpdateMissionDto dto)
        {
            var detail = await _missionService.GetDetailAsync(dto.Id, GetUserId(), GetUserRole());
            if (detail == null) return NotFound();

            var createDto = new CreateMissionDto
            {
                Title = dto.Title ?? detail.Title,
                MissionType = dto.MissionType ?? detail.MissionType,
                MissionDate = dto.MissionDate ?? detail.MissionDate,
                GovernorateId = dto.GovernorateId ?? (await _geographicService.GetGovernoratesAsync()).FirstOrDefault(g => g.NameAr == detail.GovernorateNameAr)?.Id ?? 0,
                DistrictId = dto.DistrictId,
                SubDistrictId = dto.SubDistrictId,
                TargetArea = dto.TargetArea ?? detail.TargetArea,
                TargetPropertyCount = dto.TargetPropertyCount ?? detail.TargetPropertyCount,
                Description = dto.Description ?? detail.Description,
                ExpectedCompletionDate = dto.ExpectedCompletionDate ?? detail.ExpectedCompletionDate,
                Priority = dto.Priority ?? detail.Priority,
                IsUrgent = dto.IsUrgent ?? detail.IsUrgent
            };

            await _missionService.CreateAsync(createDto, GetUserId());
            TempData["Success"] = "تم حفظ التعديلات بنجاح";
            return RedirectToAction(nameof(Detail), new { id = dto.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Assign(AssignMissionDto dto)
        {
            await _missionService.AssignAsync(dto, GetUserId());
            var mission = await _missionService.GetDetailAsync(dto.MissionId, GetUserId(), GetUserRole());
            return Json(new { success = true, data = new { newStage = mission?.Stage.ToString(), inspectorName = mission?.AssignedToName }, message = "تم الإسناد بنجاح", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> Reassign(ReassignMissionDto dto)
        {
            await _missionService.ReassignAsync(dto, GetUserId());
            var mission = await _missionService.GetDetailAsync(dto.MissionId, GetUserId(), GetUserRole());
            return Json(new { success = true, data = new { newInspectorName = mission?.AssignedToName }, message = "تمت إعادة الإسناد", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [Authorize(Roles = "FIELD_INSPECTOR")]
        public async Task<IActionResult> AcceptMission(int id)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.Accepted }, GetUserId());
            return Json(new { success = ok, data = new { stage = MissionStage.Accepted.ToString() }, message = "تم قبول المهمة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [Authorize(Roles = "FIELD_INSPECTOR")]
        public async Task<IActionResult> RejectMission(int id, string reason)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.Rejected, Notes = reason }, GetUserId());
            return Json(new { success = ok, data = (object?)null, message = "تم رفض المهمة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [Authorize(Roles = "FIELD_INSPECTOR")]
        public async Task<IActionResult> Checkin(int id, decimal lat, decimal lng)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.InProgress, CheckinLat = lat, CheckinLng = lng }, GetUserId());
            await MissionHub.BroadcastCheckin(_hub, id, lat, lng, User.Identity?.Name ?? "المفتش");
            return Json(new { success = ok, data = new { stage = MissionStage.InProgress.ToString(), checkinTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm") }, message = "تم تسجيل الوصول", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [Authorize(Roles = "FIELD_INSPECTOR")]
        public async Task<IActionResult> SubmitForReview(SubmitForReviewDto dto)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = dto.MissionId, ToStage = MissionStage.SubmittedForReview, Notes = dto.InspectorNotes }, GetUserId());
            return Json(new { success = ok, data = new { stage = MissionStage.SubmittedForReview.ToString() }, message = "تم التقديم للمراجعة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> StartReview(int id)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.UnderReview }, GetUserId());
            return Json(new { success = ok, data = (object?)null, message = "تم بدء المراجعة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(ApproveRejectDto dto)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = dto.MissionId, ToStage = MissionStage.Completed, Notes = dto.Notes }, GetUserId());
            return Json(new { success = ok, data = (object?)null, message = "تم اعتماد المهمة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> SendForCorrection(ApproveRejectDto dto)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = dto.MissionId, ToStage = MissionStage.SentForCorrection, Notes = dto.Notes }, GetUserId());
            return Json(new { success = ok, data = (object?)null, message = "تمت الإعادة للتصحيح", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(CancelMissionDto dto)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = dto.MissionId, ToStage = MissionStage.Cancelled, Notes = dto.CancellationReason }, GetUserId());
            return Json(new { success = ok, data = (object?)null, message = "تم إلغاء المهمة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [Authorize(Roles = "FIELD_INSPECTOR")]
        public async Task<IActionResult> RecordPropertyEntry(int missionId, long? propertyId, string? localId)
        {
            await _missionService.RecordPropertyEntryAsync(missionId, propertyId, localId, GetUserId());
            var mission = await _missionService.GetDetailAsync(missionId, GetUserId(), GetUserRole());
            await MissionHub.BroadcastProgressUpdate(_hub, missionId, mission?.ProgressPercent ?? 0, mission?.EnteredPropertyCount ?? 0);
            return Json(new { success = true, data = new { entryId = 0, missionProgress = mission?.ProgressPercent ?? 0, newStage = mission?.Stage.ToString() }, message = "تم تسجيل إدخال العقار", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveEntry(long entryId)
        {
            await _missionService.ApproveEntryAsync(entryId, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم اعتماد الإدخال", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> RejectEntry(long entryId, string notes)
        {
            await _missionService.RejectEntryAsync(entryId, notes, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تم رفض الإدخال", errors = Array.Empty<string>() });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesForAssignment(int governorateId)
        {
            var employees = await _missionService.GetEmployeePerformanceAsync(governorateId, DateTime.Today.AddMonths(-2), DateTime.Today, GetUserId(), GetUserRole());
            return Json(new { success = true, data = employees.Select(x => new { id = x.UserId, name = x.FullName, role = x.Role, activeMissionCount = x.InProgressMissions }), message = "", errors = Array.Empty<string>() });
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams(int governorateId)
        {
            var teams = await _missionService.GetTeamsAsync(governorateId);
            return Json(new { success = true, data = teams.Select(x => new { id = x.Id, name = x.TeamName, code = x.TeamCode, members = x.Members.Count }), message = "", errors = Array.Empty<string>() });
        }

        [HttpGet]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR,FIELD_SUPERVISOR")]
        public async Task<IActionResult> PerformanceReport(int? governorateId, DateTime? from, DateTime? to)
        {
            var f = from ?? DateTime.Today.AddMonths(-1);
            var t = to ?? DateTime.Today;
            var employees = await _missionService.GetEmployeePerformanceAsync(governorateId, f, t, GetUserId(), GetUserRole());
            var vm = new PerformanceReportViewModel
            {
                Employees = employees,
                Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                FilterGovernorateId = governorateId,
                FilterFrom = f,
                FilterTo = t,
                Summary = new PerformanceReportSummaryViewModel
                {
                    TotalEmployees = employees.Count,
                    AvgCompletionRate = employees.Any() ? employees.Average(x => x.OnTimeCompletionRate) : 0,
                    AvgDqsScore = employees.Any() ? employees.Average(x => x.AverageDqsScore) : 0,
                    TotalCompleted = employees.Sum(x => x.CompletedMissions)
                }
            };
            return View("PerformanceReport", vm);
        }

        [HttpGet]
        public async Task<IActionResult> StageHistory(int missionId)
        {
            var detail = await _missionService.GetDetailAsync(missionId, GetUserId(), GetUserRole());
            return PartialView("_StageHistory", detail?.StageHistory ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> PropertyEntries(int missionId)
        {
            var detail = await _missionService.GetDetailAsync(missionId, GetUserId(), GetUserRole());
            return PartialView("_PropertyEntries", detail?.PropertyEntries ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> MyMissions()
        {
            var filter = new MissionFilterRequest { AssignedToUserId = GetUserId(), PageSize = 50 };
            var missions = await _missionService.GetPagedAsync(filter, GetUserId(), "FIELD_INSPECTOR");
            var vm = new MissionIndexViewModel
            {
                Missions = missions,
                Filter = filter,
                Governorates = new(),
                Employees = new(),
                CurrentUserRole = "FIELD_INSPECTOR",
                TotalActive = missions.Items.Count(x => x.Stage != MissionStage.Completed && x.Stage != MissionStage.Cancelled),
                TotalOverdue = missions.Items.Count(x => x.IsOverdue),
                TotalCompletedThisMonth = missions.Items.Count(x => x.Stage == MissionStage.Completed && x.MissionDate.Month == DateTime.Today.Month)
            };
            return View("MyMissions", vm);
        }

        [HttpGet]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> Teams(int? governorateId)
        {
            var vm = new TeamsViewModel
            {
                Teams = await _missionService.GetTeamsAsync(governorateId),
                Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                FilterGovernorateId = governorateId
            };
            return View("Teams", vm);
        }

        [HttpGet]
        public async Task<IActionResult> TeamDetail(int id)
        {
            var team = await _missionService.GetTeamDetailAsync(id);
            if (team == null) return NotFound();

            var missionPage = await _missionService.GetPagedAsync(new MissionFilterRequest { PageSize = 10, SortBy = "MissionDate", SortDesc = true }, GetUserId(), GetUserRole());
            return View("TeamDetail", new TeamDetailViewModel
            {
                Team = team,
                AvailableEmployees = (await _missionService.GetEmployeePerformanceAsync(team.GovernorateId, DateTime.Today.AddMonths(-2), DateTime.Today, GetUserId(), GetUserRole()))
                    .Select(x => new SelectListItem(x.FullName, x.UserId.ToString()))
                    .ToList(),
                RecentMissions = missionPage.Items.Where(x => x.AssignedToName != null).Take(10).ToList()
            });
        }

        [HttpGet]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> CreateTeam()
        {
            ViewBag.Governorates = (await _geographicService.GetGovernoratesAsync()).Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList();
            return View("CreateTeam", new TeamCreateDto());
        }

        [HttpPost]
        [Authorize(Roles = "SYS_ADMIN,AUTH_DIRECTOR,REGIONAL_MGR")]
        public async Task<IActionResult> CreateTeam(TeamCreateDto dto)
        {
            var id = await _missionService.CreateTeamAsync(dto, GetUserId());
            return RedirectToAction(nameof(TeamDetail), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AddTeamMember(int teamId, int userId)
        {
            await _missionService.AddTeamMemberAsync(teamId, userId, GetUserId());
            var user = (await _missionService.GetEmployeePerformanceAsync(null, DateTime.Today.AddYears(-1), DateTime.Today, GetUserId(), GetUserRole())).FirstOrDefault(x => x.UserId == userId);
            return Json(new { success = true, data = new { memberName = user?.FullName ?? "" }, message = "تمت الإضافة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTeamMember(int teamId, int userId)
        {
            await _missionService.RemoveTeamMemberAsync(teamId, userId, GetUserId());
            return Json(new { success = true, data = (object?)null, message = "تمت الإزالة", errors = Array.Empty<string>() });
        }
    }
}
