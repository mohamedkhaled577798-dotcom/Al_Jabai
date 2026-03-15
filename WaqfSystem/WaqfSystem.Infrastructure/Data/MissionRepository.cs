using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Infrastructure.Data
{
    public interface IMissionRepository
    {
        Task<InspectionMission?> GetMissionWithDetailsAsync(long id);
        Task<List<InspectionMission>> GetMissionsByInspectorAsync(int userId, MissionFilterRequest filter);
        Task<List<InspectionMission>> GetMissionsInProgressAsync();
        Task<List<InspectionMission>> GetUpcomingMissionsAsync(int days);
        Task UpdateProgressCountsAsync(long missionId);
    }

    public class MissionRepository : IMissionRepository
    {
        private readonly WaqfDbContext _db;

        public MissionRepository(WaqfDbContext db)
        {
            _db = db;
        }

        public async Task<InspectionMission?> GetMissionWithDetailsAsync(long id)
        {
            return await _db.InspectionMissions
                .Include(x => x.Governorate)
                .Include(x => x.District)
                .Include(x => x.SubDistrict)
                .Include(x => x.AssignedToUser)
                .ThenInclude(x => x!.Role)
                .Include(x => x.AssignedByUser)
                .ThenInclude(x => x!.Role)
                .Include(x => x.ReviewerUser)
                .ThenInclude(x => x!.Role)
                .Include(x => x.AssignedToTeam)
                .ThenInclude(x => x!.Members)
                .Include(x => x.StageHistory)
                .ThenInclude(x => x.ChangedBy)
                .Include(x => x.PropertyEntries)
                .ThenInclude(x => x.Property)
                .Include(x => x.PropertyEntries)
                .ThenInclude(x => x.EnteredBy)
                .Include(x => x.PropertyEntries)
                .ThenInclude(x => x.ReviewedBy)
                .Include(x => x.ChecklistTemplate)
                .Include(x => x.ChecklistResults)
                .ThenInclude(x => x.CompletedBy)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<InspectionMission>> GetMissionsByInspectorAsync(int userId, MissionFilterRequest filter)
        {
            var query = _db.InspectionMissions.AsNoTracking().Where(x => x.AssignedToUserId == userId);

            if (filter.Stage.HasValue) query = query.Where(x => x.Stage == filter.Stage.Value);
            if (filter.DateFrom.HasValue) query = query.Where(x => x.MissionDate >= filter.DateFrom.Value.Date);
            if (filter.DateTo.HasValue) query = query.Where(x => x.MissionDate <= filter.DateTo.Value.Date);
            if (filter.IsUrgent.HasValue) query = query.Where(x => x.IsUrgent == filter.IsUrgent.Value);

            return await query
                .OrderByDescending(x => x.MissionDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<List<InspectionMission>> GetMissionsInProgressAsync()
        {
            return await _db.InspectionMissions
                .AsNoTracking()
                .Include(x => x.AssignedToUser)
                .Where(x => x.Stage == MissionStage.InProgress)
                .OrderByDescending(x => x.CurrentStageChangedAt)
                .ToListAsync();
        }

        public async Task<List<InspectionMission>> GetUpcomingMissionsAsync(int days)
        {
            var start = DateTime.Today;
            var end = start.AddDays(days);
            return await _db.InspectionMissions
                .AsNoTracking()
                .Include(x => x.AssignedToUser)
                .Include(x => x.Governorate)
                .Where(x => x.MissionDate >= start && x.MissionDate <= end)
                .OrderBy(x => x.MissionDate)
                .ToListAsync();
        }

        public async Task UpdateProgressCountsAsync(long missionId)
        {
            await _db.Database.ExecuteSqlRawAsync(@"
UPDATE InspectionMissions
SET EnteredPropertyCount = EnteredPropertyCount + 1,
    ProgressPercent = CASE WHEN TargetPropertyCount = 0 THEN 0
                           ELSE CAST((EnteredPropertyCount + 1) * 100.0 / NULLIF(TargetPropertyCount,0) AS decimal(5,2))
                      END,
    UpdatedAt = GETUTCDATE()
WHERE Id = {0}", missionId);
        }
    }
}
