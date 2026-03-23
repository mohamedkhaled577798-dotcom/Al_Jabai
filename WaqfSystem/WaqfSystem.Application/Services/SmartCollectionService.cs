using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Services
{
    public class SmartCollectionService : ISmartCollectionService
    {
        private readonly IAppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SmartCollectionService> _logger;
        private readonly IRevenueCollectionService _revenueCollectionService;

        public SmartCollectionService(
            IAppDbContext db,
            IMemoryCache cache,
            ILogger<SmartCollectionService> logger,
            IRevenueCollectionService revenueCollectionService)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _revenueCollectionService = revenueCollectionService;
        }

        public async Task<List<SmartSuggestionDto>> GetSuggestionsAsync(int userId, string? periodLabel)
        {
            var normalizedPeriod = string.IsNullOrWhiteSpace(periodLabel) ? DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture) : periodLabel;
            var key = $"suggestions_{userId}_{normalizedPeriod}";

            if (_cache.TryGetValue(key, out List<SmartSuggestionDto>? cached) && cached != null)
            {
                return cached;
            }

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var currentPeriod = normalizedPeriod;
            var previousPeriod = monthStart.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var contracts = await _db.RentContracts
                .AsNoTracking()
                .Include(c => c.Property)
                .Include(c => c.Floor)
                .Include(c => c.Unit)
                .Where(c => !c.IsDeleted && c.Status == ContractStatus.Active)
                .ToListAsync();

            var suggestions = new List<SmartSuggestionDto>();

            foreach (var contract in contracts)
            {
                var alreadyCollected = await _db.PropertyRevenues
                    .AsNoTracking()
                    .AnyAsync(r => !r.IsDeleted && r.ContractId == contract.Id && r.PeriodLabel == currentPeriod);
                if (alreadyCollected)
                {
                    continue;
                }

                var dueDate = contract.NextDueDate?.Date ?? today;
                var overdueDays = Math.Max(0, (today - dueDate.AddDays(contract.GracePeriodDays)).Days);
                var isDueToday = dueDate == today;
                var isDueSoon = dueDate > today && dueDate <= today.AddDays(3);
                var previousUnpaid = !await _db.PropertyRevenues
                    .AsNoTracking()
                    .AnyAsync(r => !r.IsDeleted && r.ContractId == contract.Id && r.PeriodLabel == previousPeriod);

                SuggestionType? type = null;
                var priority = 999;

                if (overdueDays > 7)
                {
                    type = SuggestionType.Overdue;
                    priority = 100 - Math.Min(90, overdueDays);
                }
                else if (overdueDays > 0)
                {
                    type = SuggestionType.Overdue;
                    priority = 220 - overdueDays;
                }
                else if (isDueToday)
                {
                    type = SuggestionType.DueToday;
                    priority = 300;
                }
                else if (isDueSoon)
                {
                    type = SuggestionType.DueSoon;
                    priority = 400 + (int)(dueDate - today).TotalDays;
                }
                else if (previousUnpaid)
                {
                    type = SuggestionType.UnpaidLastMonth;
                    priority = 500;
                }

                if (!type.HasValue)
                {
                    continue;
                }

                var dto = new SmartSuggestionDto
                {
                    SuggestionId = $"{userId}-{contract.Id}-{currentPeriod}",
                    SuggestionType = type.Value,
                    Priority = priority,
                    PropertyId = contract.PropertyId,
                    PropertyNameAr = contract.Property.PropertyName ?? "بدون اسم",
                    FloorId = contract.FloorId,
                    FloorLabel = contract.Floor?.FloorLabel,
                    UnitId = contract.UnitId,
                    UnitNumber = contract.Unit?.UnitNumber,
                    UnitType = contract.Unit?.UnitType.ToString(),
                    CollectionLevel = CollectionLevel.Unit,
                    TenantNameAr = contract.TenantNameAr,
                    TenantPhone = contract.TenantPhone,
                    ContractId = contract.Id,
                    ExpectedAmount = contract.RentAmount,
                    ContractType = contract.ContractType,
                    DueDate = dueDate,
                    OverdueDays = overdueDays,
                    PeriodLabel = currentPeriod,
                    UrgencyColor = ResolveUrgencyColor(type.Value, overdueDays)
                };

                dto.SuggestedAmountChips = BuildAmountChips(contract, overdueDays);

                var collision = await CheckCollisionAsync(dto.PropertyId, dto.CollectionLevel.ToString(), dto.FloorId, dto.UnitId, dto.PeriodLabel);
                dto.IsLocked = collision.HasCollision;
                dto.LockReason = collision.HasCollision ? collision.Message : null;

                _db.CollectionSmartLogs.Add(new CollectionSmartLog
                {
                    UserId = userId,
                    SuggestionType = dto.SuggestionType.ToString(),
                    UnitId = dto.UnitId.HasValue ? (int?)dto.UnitId.Value : null,
                    FloorId = dto.FloorId.HasValue ? (int?)dto.FloorId.Value : null,
                    PropertyId = (int)dto.PropertyId,
                    CreatedAt = DateTime.UtcNow
                });

                suggestions.Add(dto);
            }

            await _db.SaveChangesAsync();

            var result = suggestions
                .Where(x => !x.IsLocked)
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.OverdueDays)
                .Take(6)
                .ToList();

            _cache.Set(key, result, TimeSpan.FromMinutes(2));
            return result;
        }



        public async Task<TodayDashboardDto> GetTodayDashboardAsync(int userId, string periodLabel)
        {
            var key = $"dashboard_{userId}_{periodLabel}";
            if (_cache.TryGetValue(key, out TodayDashboardDto? cached) && cached != null)
            {
                return cached;
            }

            var period = ParsePeriod(periodLabel);
            var today = DateTime.Today;

            var todayQuery = _db.PropertyRevenues.AsNoTracking().Where(x => !x.IsDeleted && x.CollectionDate.Date == today);
            var monthQuery = _db.PropertyRevenues.AsNoTracking().Where(x => !x.IsDeleted && x.CollectionDate.Date >= period.Start.Date && x.CollectionDate.Date <= period.End.Date);

            var collectedToday = await todayQuery.SumAsync(x => (decimal?)x.Amount) ?? 0;
            var collectedTodayCount = await todayQuery.CountAsync();
            var monthCollected = await monthQuery.SumAsync(x => (decimal?)x.Amount) ?? 0;
            var monthExpected = await _db.RentContracts.AsNoTracking().Where(x => !x.IsDeleted && x.Status == ContractStatus.Active).SumAsync(x => (decimal?)x.RentAmount) ?? 0;
            var suggestions = await GetSuggestionsAsync(userId, periodLabel);
            var overdueQuery = _db.RentPaymentSchedules.AsNoTracking().Where(x => !x.IsDeleted && !x.IsPaid && x.DueDate.Date < today);
            var overdueCount = await overdueQuery.CountAsync();
            var overdueAmount = await overdueQuery.SumAsync(x => (decimal?)x.ExpectedAmount) ?? 0;

            var monthProgress = await BuildMonthProgressAsync(period.Start, period.End);
            var floorBreakdown = await BuildFloorBreakdownAsync(periodLabel);

            var rate = monthExpected <= 0 ? 0 : Math.Round((monthCollected / monthExpected) * 100m, 2);

            var dto = new TodayDashboardDto
            {
                CollectedToday = collectedToday,
                CollectedTodayCount = collectedTodayCount,
                MonthCollected = monthCollected,
                MonthExpected = monthExpected,
                MonthCollectionRate = rate,
                OverdueCount = overdueCount,
                OverdueAmount = overdueAmount,
                PendingCount = Math.Max(0, await _db.RentPaymentSchedules.CountAsync(x => !x.IsDeleted && !x.IsPaid && x.DueDate.Date >= today && x.DueDate.Date <= period.End.Date)),
                SmartSuggestions = suggestions,
                MonthProgress = monthProgress,
                CollectionByFloor = floorBreakdown
            };

            _cache.Set(key, dto, TimeSpan.FromMinutes(2));
            return dto;
        }

        public async Task<PagedResult<SearchResultDto>> SearchAsync(string query, int userId, int page)
        {
            var text = (query ?? string.Empty).Trim();
            var periodLabel = DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var units = _db.PropertyUnits.AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Floor)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(text))
            {
                units = units.Where(x =>
                    (x.UnitNumber != null && x.UnitNumber.Contains(text)) ||
                    (x.Property.PropertyName != null && x.Property.PropertyName.Contains(text)) ||
                    _db.RentContracts.Any(c => !c.IsDeleted && c.UnitId == x.Id && c.Status == ContractStatus.Active &&
                        ((c.TenantNameAr != null && c.TenantNameAr.Contains(text)) ||
                         (c.TenantPhone != null && c.TenantPhone.Contains(text)))));
            }

            var total = await units.CountAsync();
            var list = await units
                .OrderBy(x => x.Property.PropertyName)
                .ThenBy(x => x.UnitNumber)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            var results = new List<SearchResultDto>();

            foreach (var unit in list)
            {
                var activeContract = await _db.RentContracts.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.UnitId == unit.Id && x.Status == ContractStatus.Active)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                var isCollected = await _db.PropertyRevenues.AsNoTracking()
                    .AnyAsync(x => !x.IsDeleted && x.UnitId == unit.Id && x.PeriodLabel == periodLabel);

                var dueDate = activeContract?.NextDueDate ?? DateTime.Today;
                var overdueDays = activeContract == null ? 0 : Math.Max(0, (DateTime.Today - dueDate.AddDays(activeContract.GracePeriodDays)).Days);
                var lockResult = await CheckCollisionAsync(unit.PropertyId, "Unit", unit.FloorId, unit.Id, periodLabel);

                results.Add(new SearchResultDto
                {
                    UnitId = unit.Id,
                    UnitNumber = unit.UnitNumber ?? "-",
                    UnitType = unit.UnitType.ToString(),
                    FloorLabel = unit.Floor?.FloorLabel ?? "-",
                    PropertyNameAr = unit.Property.PropertyName ?? "بدون اسم",
                    WqfNumber = unit.Property.WqfNumber,
                    TenantNameAr = activeContract?.TenantNameAr,
                    TenantPhone = activeContract?.TenantPhone,
                    ActiveContractRent = activeContract?.RentAmount ?? 0,
                    CollectionStatusThisMonth = activeContract == null ? "شاغرة" : (lockResult.HasCollision ? "مقفول" : (isCollected ? "محصل" : (overdueDays > 0 ? "متأخر" : "قيد الانتظار"))),
                    OverdueDays = overdueDays,
                    LastCollectionDate = await _db.PropertyRevenues.AsNoTracking().Where(x => !x.IsDeleted && x.UnitId == unit.Id).OrderByDescending(x => x.CollectionDate).Select(x => (DateTime?)x.CollectionDate).FirstOrDefaultAsync(),
                    CollectionLevel = CollectionLevel.Unit,
                    IsLocked = lockResult.HasCollision,
                    LockReason = lockResult.Message,
                    PropertyId = unit.PropertyId,
                    FloorId = unit.FloorId,
                    ContractId = activeContract?.Id,
                    ExpectedAmount = activeContract?.RentAmount ?? 0,
                    PeriodLabel = periodLabel
                });
            }

            return new PagedResult<SearchResultDto>
            {
                Items = results,
                TotalCount = total,
                Page = page,
                PageSize = 20
            };
        }

        public async Task<BatchCollectResultDto> BatchCollectAsync(BatchCollectDto dto, int userId)
        {
            var batchCode = await GenerateBatchCodeAsync();
            var result = new BatchCollectResultDto { BatchCode = batchCode };

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in dto.Items)
                {
                    var unitLabel = await BuildUnitLabelAsync(item.PropertyId, item.FloorId, item.UnitId);

                    try
                    {
                        var collectDto = new CollectRevenueDto
                        {
                            PropertyId = item.PropertyId,
                            CollectionLevel = item.CollectionLevel,
                            FloorId = item.FloorId,
                            UnitId = item.UnitId,
                            ContractId = item.ContractId,
                            PeriodLabel = dto.PeriodLabel,
                            CollectionDate = dto.CollectionDate,
                            PaymentMethod = dto.PaymentMethod,
                            ReceiptNumber = item.ReceiptNumber,
                            PayerNameAr = item.PayerNameAr,
                            Amount = item.Amount,
                            ExpectedAmount = item.ExpectedAmount,
                            PeriodStartDate = ParsePeriod(dto.PeriodLabel).Start,
                            PeriodEndDate = ParsePeriod(dto.PeriodLabel).End,
                            Notes = dto.Notes
                        };

                        var collision = await _revenueCollectionService.CheckCollisionAsync(item.PropertyId, item.CollectionLevel.ToString(), item.FloorId, item.UnitId, dto.PeriodLabel);
                        if (collision.HasCollision)
                        {
                            result.Results.Add(new BatchItemResultDto
                            {
                                UnitLabel = unitLabel,
                                Amount = item.Amount,
                                Success = false,
                                Error = collision.Message
                            });
                            continue;
                        }

                        var revenue = await _revenueCollectionService.CollectAsync(collectDto, userId);
                        // BatchId will be set after saving the batch and getting its surrogate ID
                        
                        result.SuccessCount++;
                        result.TotalAmount += item.Amount;
                        result.Results.Add(new BatchItemResultDto
                        {
                            UnitLabel = unitLabel,
                            Amount = item.Amount,
                            Success = true,
                            RevenueCode = revenue.RevenueCode
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "فشل عنصر ضمن دفعة التحصيل");
                        result.Results.Add(new BatchItemResultDto
                        {
                            UnitLabel = unitLabel,
                            Amount = item.Amount,
                            Success = false,
                            Error = ex.Message
                        });
                    }
                }

                result.FailedCount = result.Results.Count(x => !x.Success);

                var batch = new CollectionBatch
                {
                    BatchCode = batchCode,
                    PeriodLabel = dto.PeriodLabel,
                    CollectedById = userId,
                    TotalAmount = result.TotalAmount,
                    ItemCount = result.SuccessCount,
                    CollectionDate = dto.CollectionDate.Date,
                    PaymentMethod = dto.PaymentMethod,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _db.CollectionBatches.Add(batch);
                await _db.SaveChangesAsync();

                // Assign regular revenues to this batch ID
                var revCodes = result.Results.Where(x => x.Success).Select(x => x.RevenueCode).ToList();
                var revenuesToUpdate = await _db.PropertyRevenues.Where(x => revCodes.Contains(x.RevenueCode)).ToListAsync();
                foreach(var r in revenuesToUpdate)
                {
                    r.BatchId = batch.Id;
                }
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                _cache.Remove($"dashboard_{userId}_{dto.PeriodLabel}");
                _cache.Remove($"suggestions_{userId}_{dto.PeriodLabel}");

                return result;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public Task<(bool HasCollision, string Message, string? LockedBy)> CheckCollisionAsync(long propertyId, string level, long? floorId, long? unitId, string period)
        {
            return _revenueCollectionService.CheckCollisionAsync((int)propertyId, level, (int?)floorId, (int?)unitId, period);
        }

        public async Task<List<SmartSuggestionDto>> GetPendingForBatchAsync(int userId, string periodLabel)
        {
            var suggestions = await GetSuggestionsAsync(userId, periodLabel);
            return suggestions.Where(x => !x.IsLocked).ToList();
        }

        private async Task<List<DayProgressDto>> BuildMonthProgressAsync(DateTime start, DateTime end)
        {
            var list = new List<DayProgressDto>();
            for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
            {
                var collected = await _db.PropertyRevenues.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.CollectionDate.Date == day)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var expected = await _db.RentPaymentSchedules.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.DueDate.Date == day)
                    .SumAsync(x => (decimal?)x.ExpectedAmount) ?? 0;

                list.Add(new DayProgressDto
                {
                    Date = day,
                    Collected = collected,
                    Expected = expected,
                    IsToday = day == DateTime.Today
                });
            }

            return list;
        }

        private async Task<List<FloorCollectionDto>> BuildFloorBreakdownAsync(string periodLabel)
        {
            var floors = await _db.PropertyFloors.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync();
            var list = new List<FloorCollectionDto>();

            foreach (var floor in floors)
            {
                var collected = await _db.PropertyRevenues.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.FloorId == floor.Id && x.PeriodLabel == periodLabel)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var expected = await _db.RentContracts.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.FloorId == floor.Id && x.Status == ContractStatus.Active)
                    .SumAsync(x => (decimal?)x.RentAmount) ?? 0;

                var unitIds = await _db.PropertyUnits.AsNoTracking().Where(x => !x.IsDeleted && x.FloorId == floor.Id).Select(x => x.Id).ToListAsync();
                var collectedUnits = await _db.PropertyRevenues.AsNoTracking()
                    .Where(x => !x.IsDeleted && x.PeriodLabel == periodLabel && x.UnitId.HasValue && unitIds.Contains(x.UnitId.Value))
                    .Select(x => x.UnitId!.Value)
                    .Distinct()
                    .CountAsync();

                list.Add(new FloorCollectionDto
                {
                    FloorLabel = floor.FloorLabel ?? $"طابق {floor.FloorNumber}",
                    Collected = collected,
                    Expected = expected,
                    Rate = expected <= 0 ? 0 : Math.Round((collected / expected) * 100m, 2),
                    UncollectedUnits = Math.Max(0, unitIds.Count - collectedUnits)
                });
            }

            return list.OrderByDescending(x => x.Expected).ToList();
        }

        private static string ResolveUrgencyColor(SuggestionType type, int overdueDays)
        {
            if (type == SuggestionType.Overdue && overdueDays > 7)
            {
                return "#B3261E";
            }

            return type switch
            {
                SuggestionType.Overdue => "#D45A1C",
                SuggestionType.DueToday => "#C58A08",
                SuggestionType.DueSoon => "#1C6FA9",
                SuggestionType.UnpaidLastMonth => "#6D5E2E",
                _ => "#2E7D32"
            };
        }

        private static List<AmountChipDto> BuildAmountChips(RentContract contract, int overdueDays)
        {
            var chips = new List<AmountChipDto>
            {
                new AmountChipDto
                {
                    Label = $"كامل {FormatAmount(contract.RentAmount)}",
                    Amount = contract.RentAmount,
                    IsDefault = true,
                    ChipType = ChipType.Full
                }
            };

            if (contract.AllowsPartialPayments)
            {
                var half = Math.Round(contract.RentAmount / 2m, 0);
                chips.Add(new AmountChipDto
                {
                    Label = $"نصف {FormatAmount(half)}",
                    Amount = half,
                    IsDefault = false,
                    ChipType = ChipType.Half
                });
            }

            if (overdueDays > 0 && contract.PenaltyPerDay.HasValue && contract.PenaltyPerDay.Value > 0)
            {
                var penaltyAmount = contract.RentAmount + (overdueDays * contract.PenaltyPerDay.Value);
                chips.Add(new AmountChipDto
                {
                    Label = $"+غرامة {FormatAmount(penaltyAmount)}",
                    Amount = penaltyAmount,
                    IsDefault = false,
                    ChipType = ChipType.WithPenalty
                });
            }

            return chips;
        }

        private static string FormatAmount(decimal amount)
        {
            return string.Format(CultureInfo.GetCultureInfo("ar-IQ"), "{0:N0} د.ع", amount);
        }

        private async Task<string> GenerateBatchCodeAsync()
        {
            var year = DateTime.Today.Year;
            var prefix = $"BCH-{year}-";
            var latest = await _db.CollectionBatches
                .AsNoTracking()
                .Where(x => x.BatchCode.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.BatchCode)
                .FirstOrDefaultAsync();

            var number = 1;
            if (!string.IsNullOrWhiteSpace(latest))
            {
                var tail = latest[prefix.Length..];
                if (int.TryParse(tail, out var n))
                {
                    number = n + 1;
                }
            }

            return $"{prefix}{number.ToString("D5", CultureInfo.InvariantCulture)}";
        }

        private async Task<string> BuildUnitLabelAsync(long propertyId, long? floorId, long? unitId)
        {
            var property = await _db.Properties.AsNoTracking().Where(x => x.Id == propertyId).Select(x => x.PropertyName).FirstOrDefaultAsync() ?? "عقار";
            var floor = floorId.HasValue ? await _db.PropertyFloors.AsNoTracking().Where(x => x.Id == floorId).Select(x => x.FloorLabel).FirstOrDefaultAsync() : null;
            var unit = unitId.HasValue ? await _db.PropertyUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitNumber).FirstOrDefaultAsync() : null;
            return $"{property} / {floor ?? "-"} / {unit ?? "-"}";
        }

        private static (DateTime Start, DateTime End) ParsePeriod(string periodLabel)
        {
            if (DateTime.TryParseExact(periodLabel, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                var start = new DateTime(parsed.Year, parsed.Month, 1);
                return (start, start.AddMonths(1).AddDays(-1));
            }

            var now = DateTime.Today;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            return (monthStart, monthStart.AddMonths(1).AddDays(-1));
        }
    }
}
