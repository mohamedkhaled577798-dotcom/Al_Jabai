using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Services
{
    public class RentContractService : IRentContractService
    {
        private readonly IAppDbContext _db;

        public RentContractService(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<RentContract>> GetPagedAsync(ContractFilterRequest filter, int userId)
        {
            var q = _db.RentContracts.AsNoTracking().Include(x => x.Property).Include(x => x.Unit).Where(x => !x.IsDeleted);
            if (filter.ActiveOnly)
            {
                q = q.Where(x => x.Status == ContractStatus.Active);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var text = filter.Search.Trim();
                q = q.Where(x =>
                    x.TenantNameAr.Contains(text) ||
                    (x.TenantPhone != null && x.TenantPhone.Contains(text)) ||
                    (x.Unit.UnitNumber != null && x.Unit.UnitNumber.Contains(text)) ||
                    (x.Property.PropertyName != null && x.Property.PropertyName.Contains(text)));
            }

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<RentContract> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
        }

        public async Task<RentContract?> GetByIdAsync(long id, int userId)
        {
            return await _db.RentContracts
                .AsNoTracking()
                .Include(x => x.PaymentSchedules)
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.Id == (int)id && !x.IsDeleted);
        }

        public async Task<RentContract> CreateAsync(CreateContractDto dto, int userId)
        {
            var contract = new RentContract
            {
                PropertyId = (int)dto.PropertyId,
                FloorId = dto.FloorId.HasValue ? (int?)dto.FloorId.Value : null,
                UnitId = (int)dto.UnitId,
                TenantNameAr = dto.TenantNameAr,
                TenantPhone = dto.TenantPhone,
                RentAmount = dto.RentAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ContractType = dto.ContractType,
                GracePeriodDays = (byte)dto.GracePeriodDays,
                AllowsPartialPayments = dto.AllowsPartialPayments,
                PenaltyPerDay = dto.PenaltyPerDay,
                Status = ContractStatus.Active,
                NextDueDate = dto.StartDate.Date,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            _db.RentContracts.Add(contract);
            await _db.SaveChangesAsync();

            var cursor = new DateTime(dto.StartDate.Year, dto.StartDate.Month, 1);
            while (cursor <= dto.EndDate.Date)
            {
                _db.RentPaymentSchedules.Add(new RentPaymentSchedule
                {
                    ContractId = contract.Id,
                    PeriodLabel = cursor.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                    DueDate = cursor,
                    ExpectedAmount = dto.RentAmount,
                    IsPaid = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                });

                cursor = cursor.AddMonths(1);
            }

            var unit = await _db.PropertyUnits.FirstOrDefaultAsync(x => x.Id == dto.UnitId && !x.IsDeleted);
            if (unit != null)
            {
                unit.CurrentContractId = contract.Id;
                unit.OccupancyStatus = OccupancyStatus.Rented;
                unit.UpdatedAt = DateTime.UtcNow;
                unit.UpdatedById = userId;
            }

            await _db.SaveChangesAsync();
            return contract;
        }

        public async Task TerminateAsync(TerminateContractDto dto, int userId)
        {
            var contract = await _db.RentContracts.FirstOrDefaultAsync(x => x.Id == (int)dto.ContractId && !x.IsDeleted);
            if (contract == null)
            {
                throw new InvalidOperationException("العقد غير موجود");
            }

            contract.Status = ContractStatus.Terminated;
            contract.EndDate = dto.TerminationDate.Date;
            contract.Notes = string.IsNullOrWhiteSpace(dto.Reason) ? contract.Notes : $"{contract.Notes} | إنهاء: {dto.Reason}";
            contract.UpdatedAt = DateTime.UtcNow;
            contract.UpdatedById = userId;

            var unit = await _db.PropertyUnits.FirstOrDefaultAsync(x => x.Id == contract.UnitId && !x.IsDeleted);
            if (unit != null && unit.CurrentContractId == contract.Id)
            {
                unit.CurrentContractId = null;
                unit.OccupancyStatus = OccupancyStatus.Vacant;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<RentContract?> GetActiveByUnitIdAsync(long unitId, int userId)
        {
            return await _db.RentContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UnitId == unitId && x.Status == ContractStatus.Active);
        }

        public async Task<List<RentContract>> GetExpiringAsync(int days, int userId)
        {
            var date = DateTime.Today.AddDays(days);
            return await _db.RentContracts
                .AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted && x.Status == ContractStatus.Active && x.EndDate.Date <= date)
                .OrderBy(x => x.EndDate)
                .ToListAsync();
        }

        public async Task<List<RentPaymentSchedule>> GetScheduleAsync(long contractId, int userId)
        {
            return await _db.RentPaymentSchedules
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.ContractId == contractId)
                .OrderBy(x => x.DueDate)
                .ToListAsync();
        }
    }
}
