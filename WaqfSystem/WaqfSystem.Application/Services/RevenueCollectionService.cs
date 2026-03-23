using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;


namespace WaqfSystem.Application.Services
{
    public class RevenueCollectionService : IRevenueCollectionService
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<RevenueCollectionService> _logger;

        public RevenueCollectionService(
            IAppDbContext db,
            ILogger<RevenueCollectionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PagedResult<PropertyRevenue>> GetPagedAsync(RevenueFilterRequest filter, int userId)
        {
            var q = _db.PropertyRevenues
                .AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.PeriodLabel))
            {
                q = q.Where(x => x.PeriodLabel == filter.PeriodLabel);
            }

            if (filter.FromDate.HasValue)
            {
                q = q.Where(x => x.CollectionDate.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                q = q.Where(x => x.CollectionDate.Date <= filter.ToDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var text = filter.Search.Trim();
                q = q.Where(x =>
                    x.RevenueCode.Contains(text) ||
                    (x.PayerNameAr != null && x.PayerNameAr.Contains(text)) ||
                    (x.ReceiptNumber != null && x.ReceiptNumber.Contains(text)) ||
                    (x.Property.PropertyName != null && x.Property.PropertyName.Contains(text)) ||
                    (x.Unit != null && x.Unit.UnitNumber != null && x.Unit.UnitNumber.Contains(text)));
            }

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.CollectionDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<PropertyRevenue>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PropertyRevenue> CollectAsync(CollectRevenueDto dto, int userId)
        {
            var collision = await CheckCollisionAsync(dto.PropertyId, dto.CollectionLevel.ToString(), dto.FloorId, dto.UnitId, dto.PeriodLabel);
            if (collision.HasCollision)
            {
                throw new CollisionException(collision.Message, collision.LockedBy);
            }

            var revenue = new PropertyRevenue
            {
                RevenueCode = await GenerateRevenueCodeAsync(),
                PropertyId = (int)dto.PropertyId,
                FloorId = dto.FloorId.HasValue ? (int?)dto.FloorId.Value : null,
                UnitId = dto.UnitId.HasValue ? (int?)dto.UnitId.Value : null,
                ContractId = dto.ContractId.HasValue ? (int?)dto.ContractId.Value : null,
                CollectionLevel = dto.CollectionLevel,
                PeriodLabel = dto.PeriodLabel,
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                CollectionDate = dto.CollectionDate,
                Amount = dto.Amount,
                ExpectedAmount = dto.ExpectedAmount,
                PaymentMethod = dto.PaymentMethod,
                ReceiptNumber = dto.ReceiptNumber,
                PayerNameAr = dto.PayerNameAr,
                Notes = dto.Notes,
                CollectedById = userId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow
            };

            _db.PropertyRevenues.Add(revenue);
            await _db.SaveChangesAsync();

            await ApplyLockAfterCollectionAsync(revenue);
            await _db.SaveChangesAsync();

            return revenue;
        }

        public async Task<PropertyRevenue> QuickCollectAsync(QuickCollectDto dto, int userId)
        {
            var range = (dto.PeriodStartDate == default || dto.PeriodEndDate == default) 
                ? ParsePeriod(dto.PeriodLabel) 
                : (Start: dto.PeriodStartDate.Value, End: dto.PeriodEndDate.Value);

            if (dto.PeriodStartDate == default || dto.PeriodEndDate == default)
            {
                dto.PeriodStartDate = range.Start;
                dto.PeriodEndDate = range.End;
            }

            var variance = await PreviewVarianceAsync(dto.ContractId, dto.Amount);
            if (Math.Abs(variance.VariancePercent) > 10m && string.IsNullOrWhiteSpace(dto.VarianceApprovalNote))
            {
                throw new InvalidOperationException("الفارق أكبر من 10%، يجب إضافة ملاحظة الموافقة");
            }

            var collectDto = new CollectRevenueDto
            {
                PropertyId = dto.PropertyId,
                CollectionLevel = dto.CollectionLevel,
                FloorId = dto.FloorId,
                UnitId = dto.UnitId,
                ContractId = dto.ContractId,
                PeriodLabel = dto.PeriodLabel,
                PeriodStartDate = dto.PeriodStartDate ?? range.Start,
                PeriodEndDate = dto.PeriodEndDate ?? range.End,
                CollectionDate = dto.CollectionDate,
                Amount = dto.Amount,
                ExpectedAmount = dto.ExpectedAmount,
                PaymentMethod = dto.PaymentMethod,
                ReceiptNumber = dto.ReceiptNumber,
                PayerNameAr = dto.PayerNameAr,
                Notes = dto.Notes
            };

            var revenue = await CollectAsync(collectDto, userId);
            revenue.SuggestedBySystem = !string.IsNullOrWhiteSpace(dto.SuggestionId);
            revenue.VarianceApprovalNote = dto.VarianceApprovalNote;
            if (!string.IsNullOrWhiteSpace(dto.VarianceApprovalNote))
            {
                revenue.VarianceApprovedBy = userId;
            }

            if (!string.IsNullOrWhiteSpace(dto.SuggestionId))
            {
                var log = await _db.CollectionSmartLogs
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync(x => x.UserId == userId && !x.WasActedOn &&
                                              x.PropertyId == (int)dto.PropertyId &&
                                              x.UnitId == (dto.UnitId.HasValue ? (int?)dto.UnitId.Value : null));
                if (log != null)
                {
                    log.WasActedOn = true;
                }
            }

            await _db.SaveChangesAsync();
            return revenue;
        }

        public async Task<VarianceAlertDto> PreviewVarianceAsync(long? contractId, decimal amount)
        {
            decimal expected = 0;

            if (contractId.HasValue)
            {
                expected = await _db.RentContracts
                    .AsNoTracking()
                    .Where(x => x.Id == (int)contractId.Value && !x.IsDeleted)
                    .Select(x => x.RentAmount)
                    .FirstOrDefaultAsync();
            }

            if (expected <= 0)
            {
                return new VarianceAlertDto
                {
                    ExpectedAmount = expected,
                    ActualAmount = amount,
                    Variance = amount,
                    VariancePercent = 0,
                    AlertLevel = AlertLevel.None,
                    AlertMessage = "لا يمكن حساب الفارق لعدم توفر مبلغ متوقع",
                    RequiresApproval = false
                };
            }

            var variance = amount - expected;
            var pct = Math.Round((variance / expected) * 100m, 2);
            var absPct = Math.Abs(pct);

            if (absPct < 1m)
            {
                return new VarianceAlertDto
                {
                    ExpectedAmount = expected,
                    ActualAmount = amount,
                    Variance = variance,
                    VariancePercent = pct,
                    AlertLevel = AlertLevel.None,
                    AlertMessage = "لا يوجد فرق مؤثر",
                    RequiresApproval = false
                };
            }

            if (absPct <= 10m)
            {
                return new VarianceAlertDto
                {
                    ExpectedAmount = expected,
                    ActualAmount = amount,
                    Variance = variance,
                    VariancePercent = pct,
                    AlertLevel = AlertLevel.Warning,
                    AlertMessage = "إشعار بالفارق - لا يمنع التسجيل",
                    RequiresApproval = false
                };
            }

            var criticalMessage = absPct > 20m
                ? "فارق حرج يتطلب ملاحظة وتدقيق إداري"
                : "فارق كبير يتطلب ملاحظة موافقة";

            return new VarianceAlertDto
            {
                ExpectedAmount = expected,
                ActualAmount = amount,
                Variance = variance,
                VariancePercent = pct,
                AlertLevel = AlertLevel.Critical,
                AlertMessage = criticalMessage,
                RequiresApproval = true
            };
        }

        public async Task<(bool HasCollision, string Message, string? LockedBy)> CheckCollisionAsync(long propertyId, string level, long? floorId, long? unitId, string periodLabel)
        {
            var locks = _db.RevenuePeriodLocks.AsNoTracking().Where(x => x.PropertyId == (int)propertyId && x.PeriodLabel == periodLabel);

            if (level.Equals("Building", StringComparison.OrdinalIgnoreCase))
            {
                var exists = await locks.AnyAsync();
                if (exists)
                {
                    var info = await locks.OrderByDescending(x => x.Id).FirstAsync();
                    return (true, $"تم تحصيل جزء من العقار في هذه الفترة: {info.ReasonAr}", info.LockedByRevenueCode);
                }
                return (false, "لا يوجد تعارض", null);
            }

            if (level.Equals("Floor", StringComparison.OrdinalIgnoreCase))
            {
                var exists = await locks.AnyAsync(x =>
                    x.LockedLevel == CollectionLevel.Building ||
                    (x.LockedLevel == CollectionLevel.Floor && x.FloorId == (int?)floorId) ||
                    (x.LockedLevel == CollectionLevel.Unit && x.FloorId == (int?)floorId));
                if (exists)
                {
                    var info = await locks.OrderByDescending(x => x.Id).FirstAsync();
                    return (true, $"الطابق مقفول للفترة الحالية: {info.ReasonAr}", info.LockedByRevenueCode);
                }
                return (false, "لا يوجد تعارض", null);
            }

            var unitConflict = await locks.AnyAsync(x =>
                x.LockedLevel == CollectionLevel.Building ||
                (x.LockedLevel == CollectionLevel.Floor && x.FloorId == (int?)floorId) ||
                (x.LockedLevel == CollectionLevel.Unit && x.UnitId == (int?)unitId));

            if (unitConflict)
            {
                var info = await locks.OrderByDescending(x => x.Id).FirstAsync();
                return (true, $"الوحدة مقفولة للفترة الحالية: {info.ReasonAr}", info.LockedByRevenueCode);
            }

            return (false, "لا يوجد تعارض", null);
        }

        public async Task<List<PropertyRevenue>> GetRecentAsync(int userId, int take = 20)
        {
            return await _db.PropertyRevenues
                .AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CollectionDate)
                .ThenByDescending(x => x.Id)
                .Take(take)
                .ToListAsync();
        }

        public async Task DeleteTodayAsync(long revenueId, string reason, int userId)
        {
            var revenue = await _db.PropertyRevenues.FirstOrDefaultAsync(x => x.Id == (int)revenueId && !x.IsDeleted);
            if (revenue == null)
            {
                throw new InvalidOperationException("السجل غير موجود");
            }

            if (revenue.CollectionDate.Date != DateTime.Today)
            {
                throw new InvalidOperationException("يمكن حذف سجلات اليوم فقط");
            }

            revenue.IsDeleted = true;
            revenue.UpdatedAt = DateTime.UtcNow;
            revenue.UpdatedById = userId;
            revenue.Notes = string.IsNullOrWhiteSpace(reason) ? revenue.Notes : $"{revenue.Notes} | حذف: {reason}";

            var locks = _db.RevenuePeriodLocks.Where(x => x.LockedByRevenueCode == revenue.RevenueCode);
            _db.RevenuePeriodLocks.RemoveRange(locks);

            await _db.SaveChangesAsync();
        }

        private async Task ApplyLockAfterCollectionAsync(PropertyRevenue revenue)
        {
            var locks = new List<RevenuePeriodLock>();

            if (revenue.CollectionLevel == CollectionLevel.Building)
            {
                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Building,
                    ReasonAr = "تم تحصيل العقار بالكامل",
                    LockedByRevenueCode = revenue.RevenueCode
                });

                var floors = await _db.PropertyFloors.Where(x => x.PropertyId == revenue.PropertyId && !x.IsDeleted).ToListAsync();
                locks.AddRange(floors.Select(f => new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = f.Id,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Floor,
                    ReasonAr = "مقفول بسبب تحصيل العقار بالكامل",
                    LockedByRevenueCode = revenue.RevenueCode
                }));

                var units = await _db.PropertyUnits.Where(x => x.PropertyId == revenue.PropertyId && !x.IsDeleted).ToListAsync();
                locks.AddRange(units.Select(u => new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = u.FloorId,
                    UnitId = u.Id,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Unit,
                    ReasonAr = "مقفولة بسبب تحصيل العقار بالكامل",
                    LockedByRevenueCode = revenue.RevenueCode
                }));
            }
            else if (revenue.CollectionLevel == CollectionLevel.Floor && revenue.FloorId.HasValue)
            {
                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = revenue.FloorId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Floor,
                    ReasonAr = "تم تحصيل الطابق",
                    LockedByRevenueCode = revenue.RevenueCode
                });

                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Building,
                    ReasonAr = "العقار محجوز بسبب تحصيل أحد الطوابق",
                    LockedByRevenueCode = revenue.RevenueCode
                });

                var units = await _db.PropertyUnits.Where(x => x.FloorId == revenue.FloorId && !x.IsDeleted).ToListAsync();
                locks.AddRange(units.Select(u => new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = revenue.FloorId,
                    UnitId = u.Id,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Unit,
                    ReasonAr = "مقفولة بسبب تحصيل الطابق",
                    LockedByRevenueCode = revenue.RevenueCode
                }));
            }
            else if (revenue.CollectionLevel == CollectionLevel.Unit && revenue.UnitId.HasValue)
            {
                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = revenue.FloorId,
                    UnitId = revenue.UnitId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Unit,
                    ReasonAr = "تم تحصيل الوحدة",
                    LockedByRevenueCode = revenue.RevenueCode
                });

                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    FloorId = revenue.FloorId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Floor,
                    ReasonAr = "الطابق محجوز بسبب تحصيل وحدة",
                    LockedByRevenueCode = revenue.RevenueCode
                });

                locks.Add(new RevenuePeriodLock
                {
                    PropertyId = revenue.PropertyId,
                    PeriodLabel = revenue.PeriodLabel,
                    LockedLevel = CollectionLevel.Building,
                    ReasonAr = "العقار محجوز بسبب تحصيل وحدة",
                    LockedByRevenueCode = revenue.RevenueCode
                });
            }

            if (locks.Count > 0)
            {
                _db.RevenuePeriodLocks.AddRange(locks);
            }
        }

        private async Task<string> GenerateRevenueCodeAsync()
        {
            var year = DateTime.Today.Year;
            var prefix = $"REV-{year}-";
            var latest = await _db.PropertyRevenues
                .AsNoTracking()
                .Where(x => x.RevenueCode.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.RevenueCode)
                .FirstOrDefaultAsync();

            var number = 1;
            if (!string.IsNullOrWhiteSpace(latest) && latest.Length > prefix.Length)
            {
                var tail = latest[prefix.Length..];
                if (int.TryParse(tail, out var parsed))
                {
                    number = parsed + 1;
                }
            }

            return $"{prefix}{number.ToString("D5", CultureInfo.InvariantCulture)}";
        }

        private static (DateTime Start, DateTime End) ParsePeriod(string periodLabel)
        {
            if (DateTime.TryParseExact(periodLabel, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                var start = new DateTime(dt.Year, dt.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return (start, end);
            }

            var now = DateTime.Today;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            return (monthStart, monthStart.AddMonths(1).AddDays(-1));
        }
    }
}
