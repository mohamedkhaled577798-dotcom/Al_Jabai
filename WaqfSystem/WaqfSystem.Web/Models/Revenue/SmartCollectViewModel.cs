using System.Collections.Generic;
using WaqfSystem.Application.DTOs.Revenue;

namespace WaqfSystem.Web.Models.Revenue
{
    public class SmartCollectViewModel
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public TodayDashboardDto Dashboard { get; set; } = new();
        public QuickCollectDto QuickCollect { get; set; } = new();
        public BatchCollectDto BatchCollect { get; set; } = new();
        public List<SmartSuggestionDto> Suggestions { get; set; } = new();
        public List<string> PaymentMethods { get; set; } = new() { "نقدي", "تحويل", "بطاقة", "شيك" };
    }
}
