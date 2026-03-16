using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public sealed class GeographicScopeContext
    {
        public bool HasGlobalScope { get; set; }
        public GeographicScopeLevel RoleScopeLevel { get; set; } = GeographicScopeLevel.None;
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public HashSet<int> AllowedGovernorateIds { get; set; } = new();
        public HashSet<int> AllowedDistrictIds { get; set; } = new();
        public HashSet<int> AllowedSubDistrictIds { get; set; } = new();
    }

    public interface IGeographicScopeService
    {
        Task<GeographicScopeContext> BuildScopeAsync(int userId, string? userRole = null);
        IQueryable<InspectionMission> ApplyToMissions(IQueryable<InspectionMission> query, GeographicScopeContext scope);
        IQueryable<Property> ApplyToProperties(IQueryable<Property> query, GeographicScopeContext scope);
    }

    public class GeographicScopeService : IGeographicScopeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GeographicScopeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GeographicScopeContext> BuildScopeAsync(int userId, string? userRole = null)
        {
            var user = await _unitOfWork.GetQueryable<User>()
                .AsNoTracking()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

            if (user == null || user.Role == null)
            {
                return new GeographicScopeContext { HasGlobalScope = true };
            }

            var roleLevel = user.Role.GeographicScopeLevel;
            var isGlobal = user.Role.HasGlobalScope || roleLevel == GeographicScopeLevel.None || userRole == "SYS_ADMIN";

            var context = new GeographicScopeContext
            {
                HasGlobalScope = isGlobal,
                RoleScopeLevel = roleLevel,
                GovernorateId = user.GovernorateId,
                DistrictId = user.DistrictId,
                SubDistrictId = user.SubDistrictId
            };

            if (context.HasGlobalScope)
            {
                return context;
            }

            var assignments = await _unitOfWork.GetQueryable<UserGeographicScope>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.IsActive)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Id)
                .ToListAsync();

            foreach (var item in assignments)
            {
                if (item.GovernorateId.HasValue) context.AllowedGovernorateIds.Add(item.GovernorateId.Value);
                if (item.DistrictId.HasValue) context.AllowedDistrictIds.Add(item.DistrictId.Value);
                if (item.SubDistrictId.HasValue) context.AllowedSubDistrictIds.Add(item.SubDistrictId.Value);
            }

            var primary = assignments.FirstOrDefault();
            if (!context.GovernorateId.HasValue) context.GovernorateId = primary?.GovernorateId;
            if (!context.DistrictId.HasValue) context.DistrictId = primary?.DistrictId;
            if (!context.SubDistrictId.HasValue) context.SubDistrictId = primary?.SubDistrictId;

            if (roleLevel == GeographicScopeLevel.Governorate)
            {
                if (context.AllowedGovernorateIds.Count > 0 && (!context.GovernorateId.HasValue || !context.AllowedGovernorateIds.Contains(context.GovernorateId.Value)))
                {
                    context.GovernorateId = context.AllowedGovernorateIds.First();
                }

                context.DistrictId = null;
                context.SubDistrictId = null;
            }
            else if (roleLevel == GeographicScopeLevel.District)
            {
                if (context.AllowedDistrictIds.Count > 0 && (!context.DistrictId.HasValue || !context.AllowedDistrictIds.Contains(context.DistrictId.Value)))
                {
                    context.DistrictId = context.AllowedDistrictIds.First();
                }

                context.SubDistrictId = null;
            }
            else if (roleLevel == GeographicScopeLevel.SubDistrict)
            {
                if (context.AllowedSubDistrictIds.Count > 0 && (!context.SubDistrictId.HasValue || !context.AllowedSubDistrictIds.Contains(context.SubDistrictId.Value)))
                {
                    context.SubDistrictId = context.AllowedSubDistrictIds.First();
                }
            }

            return context;
        }

        public IQueryable<InspectionMission> ApplyToMissions(IQueryable<InspectionMission> query, GeographicScopeContext scope)
        {
            if (scope.HasGlobalScope)
            {
                return query;
            }

            if (scope.RoleScopeLevel == GeographicScopeLevel.SubDistrict)
            {
                if (scope.AllowedSubDistrictIds.Count > 0)
                {
                    return query.Where(x => x.SubDistrictId.HasValue && scope.AllowedSubDistrictIds.Contains(x.SubDistrictId.Value));
                }

                if (scope.SubDistrictId.HasValue)
                {
                    return query.Where(x => x.SubDistrictId == scope.SubDistrictId.Value);
                }
            }

            if (scope.RoleScopeLevel == GeographicScopeLevel.District)
            {
                if (scope.AllowedDistrictIds.Count > 0)
                {
                    return query.Where(x => x.DistrictId.HasValue && scope.AllowedDistrictIds.Contains(x.DistrictId.Value));
                }

                if (scope.DistrictId.HasValue)
                {
                    return query.Where(x => x.DistrictId == scope.DistrictId.Value);
                }
            }

            if (scope.AllowedGovernorateIds.Count > 0)
            {
                return query.Where(x => scope.AllowedGovernorateIds.Contains(x.GovernorateId));
            }

            if (scope.GovernorateId.HasValue)
            {
                return query.Where(x => x.GovernorateId == scope.GovernorateId.Value);
            }

            return query;
        }

        public IQueryable<Property> ApplyToProperties(IQueryable<Property> query, GeographicScopeContext scope)
        {
            if (scope.HasGlobalScope)
            {
                return query;
            }

            if (scope.RoleScopeLevel == GeographicScopeLevel.SubDistrict)
            {
                if (scope.AllowedSubDistrictIds.Count > 0)
                {
                    return query.Where(x => x.Address != null && x.Address.Street != null && scope.AllowedSubDistrictIds.Contains(x.Address.Street.Neighborhood.SubDistrictId));
                }

                if (scope.SubDistrictId.HasValue)
                {
                    return query.Where(x => x.Address != null && x.Address.Street != null && x.Address.Street.Neighborhood.SubDistrictId == scope.SubDistrictId.Value);
                }
            }

            if (scope.RoleScopeLevel == GeographicScopeLevel.District)
            {
                if (scope.AllowedDistrictIds.Count > 0)
                {
                    return query.Where(x => x.Address != null && x.Address.Street != null && scope.AllowedDistrictIds.Contains(x.Address.Street.Neighborhood.SubDistrict.DistrictId));
                }

                if (scope.DistrictId.HasValue)
                {
                    return query.Where(x => x.Address != null && x.Address.Street != null && x.Address.Street.Neighborhood.SubDistrict.DistrictId == scope.DistrictId.Value);
                }
            }

            if (scope.AllowedGovernorateIds.Count > 0)
            {
                return query.Where(x => x.GovernorateId.HasValue && scope.AllowedGovernorateIds.Contains(x.GovernorateId.Value));
            }

            if (scope.GovernorateId.HasValue)
            {
                return query.Where(x => x.GovernorateId == scope.GovernorateId.Value);
            }

            return query;
        }
    }
}
