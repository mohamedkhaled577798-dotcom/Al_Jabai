using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Services
{
    public interface IMissionService
    {
        string GenerateMissionCode();

        Task<int> CreateAsync(CreateMissionDto dto, int userId);
        Task AssignAsync(AssignMissionDto dto, int userId);
        Task ReassignAsync(ReassignMissionDto dto, int userId);
        Task<bool> AdvanceStageAsync(AdvanceStageDto dto, int userId);
        Task RecordPropertyEntryAsync(int missionId, long? propertyId, string? localId, int userId);
        Task ApproveEntryAsync(long entryId, int userId);
        Task RejectEntryAsync(long entryId, string notes, int userId);

        Task<PagedResult<MissionListItemDto>> GetPagedAsync(MissionFilterRequest filter, int userId, string userRole);
        Task<MissionDetailDto?> GetDetailAsync(int id, int userId, string userRole);
        Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string userRole);
        Task<List<CalendarEventDto>> GetCalendarEventsAsync(int year, int month, int userId, string userRole);
        Task<List<EmployeePerformanceDto>> GetEmployeePerformanceAsync(int? governorateId, DateTime from, DateTime to, int userId, string userRole);

        Task<TeamDetailDto?> GetTeamDetailAsync(int teamId);
        Task<List<TeamDetailDto>> GetTeamsAsync(int? governorateId);
        Task<int> CreateTeamAsync(TeamCreateDto dto, int userId);
        Task AddTeamMemberAsync(int teamId, int userId, int addedById);
        Task RemoveTeamMemberAsync(int teamId, int userId, int removedById);

        // Legacy methods kept for compatibility with old endpoints.
        Task<InspectionMission> CreateMissionAsync(string title, int? governorateId, int? supervisorId, int? assignedToId, MissionType type);
        Task<List<InspectionMission>> GetMyMissionsAsync(int userId);
        Task<bool> StartMissionAsync(int missionId, decimal lat, decimal lng);
        Task<bool> CompleteMissionAsync(int missionId, string notes);
        Task<bool> UpdateProgressAsync(int missionId, int completedCount);
    }
}
