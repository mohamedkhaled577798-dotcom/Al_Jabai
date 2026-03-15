using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IPartnerCommunicationService
    {
        string BuildDistributionEmailHtml(PartnerRevenueDistribution dist, PropertyPartnership p);
        string BuildExpiryWarningEmailHtml(PropertyPartnership p, int daysRemaining);
        string BuildTransferConfirmationEmailHtml(PartnerRevenueDistribution dist, PropertyPartnership p);
        string BuildSmsText(string eventType, PropertyPartnership p, decimal? amount = null);
        Task<bool> SendDistributionNotificationAsync(PartnerRevenueDistribution dist, PropertyPartnership p, int userId);
        Task<bool> SendExpiryWarningAsync(PropertyPartnership p, int daysRemaining, int triggeredByUserId);
        Task<bool> SendTransferConfirmationAsync(PartnerRevenueDistribution dist, PropertyPartnership p, int userId);
        Task<bool> SendManualMessageAsync(SendCommunicationDto dto, PropertyPartnership p, int userId);
    }
}
