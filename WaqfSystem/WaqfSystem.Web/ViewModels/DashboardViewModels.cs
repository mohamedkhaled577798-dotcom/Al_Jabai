using System.Collections.Generic;
using WaqfSystem.Application.DTOs.Property;

namespace WaqfSystem.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProperties { get; set; }
        public int PendingApproval { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public decimal TotalEstimatedValue { get; set; }
        public decimal AverageDqsScore { get; set; }

        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
        public List<GovernorateStatsViewModel> GovernorateStats { get; set; } = new();
        public List<PropertyTypeStatsViewModel> PropertyTypeStats { get; set; } = new();
        
        public int ActiveMissions { get; set; }
        public int NotificationsCount { get; set; }
    }

    public class RecentActivityViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ActionAr { get; set; } = string.Empty;
        public string UserAr { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; }
        public string RelativeTime { get; set; } = string.Empty;
    }

    public class GovernorateStatsViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PropertyTypeStatsViewModel
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
