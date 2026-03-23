using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Document;

namespace WaqfSystem.Application.Services
{
    public interface IDocumentAlertService
    {
        Task CheckAndCreateAlertsAsync(long documentId);
        Task<int> ProcessAllExpiryChecksAsync();
        Task<List<DocumentAlertDto>> GetUnreadAlertsForUserAsync(int userId);
        Task<int> GetUnreadAlertCountAsync(int userId);
    }
}
