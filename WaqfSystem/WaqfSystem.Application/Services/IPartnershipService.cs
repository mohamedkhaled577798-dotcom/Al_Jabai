using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Partnership;

namespace WaqfSystem.Application.Services
{
    public interface IPartnershipService
    {
        Task<int> CreateAsync(CreatePartnershipDto dto, IFormFile? agreementFile, int userId);
        Task UpdateAsync(UpdatePartnershipDto dto, IFormFile? agreementFile, int userId);
        Task DeactivateAsync(int id, string reason, int userId);
        Task<RevenueDistributionDto> RecordDistributionAsync(RevenueDistributionCreateDto dto, int userId);
        Task MarkTransferredAsync(int distributionId, string method, string reference, int userId);
        Task<RevenueCalculationResultDto> PreviewRevenueCalculationAsync(int partnershipId, decimal totalRevenue, decimal totalExpenses = 0m, string? distributionType = null, string? seasonLabel = null);
        Task<string> SendCommunicationAsync(SendCommunicationDto dto, int userId);
        Task ProcessExpiryNotificationsAsync();
        Task<PartnerStatementDto> GetStatementAsync(int partnershipId, DateTime from, DateTime to);
        Task<List<PartnershipDetailDto>> GetByPropertyAsync(int propertyId);
        Task<PartnershipDetailDto?> GetByIdAsync(int id);
        Task<PagedResult<PartnershipListItemDto>> GetPagedAsync(PartnershipFilterRequest filter);
        Task<List<PartnerContactDto>> GetContactHistoryAsync(int partnershipId, int page, int pageSize);
        Task<List<RevenueDistributionDto>> GetDistributionHistoryAsync(int partnershipId);
        Task<PartnershipExpenseEntryDto> AddExpenseAsync(CreatePartnershipExpenseDto dto, int userId);
        Task<List<PartnershipExpenseEntryDto>> GetExpensesAsync(int partnershipId, DateTime? from = null, DateTime? to = null);
        Task<List<PartnershipListItemDto>> GetExpiringAsync(int daysAhead);

        // Partner Registry
        Task<int> CreatePartnerAsync(CreatePartnerDto dto, int userId);
        Task<List<PartnerSummaryDto>> SearchPartnersAsync(string term);
    }
}
