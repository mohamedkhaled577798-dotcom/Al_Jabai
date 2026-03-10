using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة إنشاء وإدارة التنبيهات والإنذارات التلقائية
/// </summary>
public class AlertService
{
    private readonly IUnitOfWork _unitOfWork;

    public AlertService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =================== توليد التنبيهات ===================

    /// <summary>
    /// يُشغَّل دورياً (BackgroundService) لتوليد جميع التنبيهات التلقائية
    /// </summary>
    public async Task GenerateAllAlertsAsync()
    {
        await GenerateContractExpiryAlertsAsync();
        await GenerateLatePaymentAlertsAsync();
        await GenerateLegalDeadlineAlertsAsync();
        await GenerateMaintenanceDueAlertsAsync();
        await GenerateEncroachmentAlertsAsync();
    }

    // ---- تنبيهات انتهاء العقود ----
    public async Task GenerateContractExpiryAlertsAsync()
    {
        var now = DateTime.Today;
        var contracts = await _unitOfWork.Repository<InvestmentContract>().Query()
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        foreach (var contract in contracts)
        {
            var daysLeft = (contract.EndDate - now).Days;

            // تحديد مستوى التنبيه
            string? severity = null;
            string? alertType = null;

            if (daysLeft < 0)
            { severity = "حرج";    alertType = "ContractExpired"; }
            else if (daysLeft <= 30)
            { severity = "حرج";    alertType = "ContractExpiry"; }
            else if (daysLeft <= 90)
            { severity = "تحذير";  alertType = "ContractExpiry"; }
            else if (daysLeft <= 180)
            { severity = "تنبيه";  alertType = "ContractExpiry"; }

            if (severity == null) continue;

            // لا تُكرر تنبيه اليوم نفسه
            bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                .AnyAsync(a => a.EntityType == "InvestmentContract"
                            && a.EntityId == contract.Id
                            && a.AlertType == alertType
                            && a.CreatedAt.Date == now
                            && !a.IsDeleted);
            if (exists) continue;

            var msg = daysLeft < 0
                ? $"انتهت صلاحية عقد [{contract.ContractNumber}] للمستثمر {contract.InvestorName} منذ {Math.Abs(daysLeft)} يوم"
                : $"عقد [{contract.ContractNumber}] للمستثمر {contract.InvestorName} ينتهي خلال {daysLeft} يوم بتاريخ {contract.EndDate:dd/MM/yyyy}";

            await CreateAlertAsync(new AlertNotification
            {
                AlertType  = alertType,
                Severity   = severity,
                Title      = daysLeft < 0 ? "عقد منتهٍ" : "عقد يوشك على الانتهاء",
                Message    = msg,
                EntityType = "InvestmentContract",
                EntityId   = contract.Id,
                EntityName = contract.ContractNumber,
                ActionUrl  = $"/Contracts/Details/{contract.Id}",
                DueDate    = contract.EndDate,
                DaysLeft   = daysLeft
            });
        }
    }

    // ---- تنبيهات الدفعات المتأخرة ----
    public async Task GenerateLatePaymentAlertsAsync()
    {
        var now = DateTime.Today;
        var latePayments = await _unitOfWork.Repository<ContractPayment>().Query()
            .Include(p => p.Contract)
            .Where(p => p.Status == "معلق" && p.DueDate < now && !p.Contract.IsDeleted)
            .ToListAsync();

        foreach (var payment in latePayments)
        {
            var daysLate = (now - payment.DueDate).Days;

            bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                .AnyAsync(a => a.EntityType == "ContractPayment"
                            && a.EntityId == payment.Id
                            && a.AlertType == "LatePayment"
                            && a.CreatedAt.Date == now
                            && !a.IsDeleted);
            if (exists) continue;

            await CreateAlertAsync(new AlertNotification
            {
                AlertType  = "LatePayment",
                Severity   = daysLate > 30 ? "حرج" : "تحذير",
                Title      = "دفعة متأخرة",
                Message    = $"دفعة رقم {payment.PaymentNumber} بمبلغ {payment.AmountDue:N0} د.ع للعقد [{payment.Contract.ContractNumber}] متأخرة {daysLate} يوم",
                EntityType = "InvestmentContract",
                EntityId   = payment.ContractId,
                EntityName = payment.Contract.ContractNumber,
                ActionUrl  = $"/Contracts/Details/{payment.ContractId}",
                DueDate    = payment.DueDate,
                DaysLeft   = -daysLate
            });
        }
    }

    // ---- تنبيهات الدعاوى القضائية ----
    public async Task GenerateLegalDeadlineAlertsAsync()
    {
        var now = DateTime.Today;
        var disputes = await _unitOfWork.Repository<LegalDispute>().Query()
            .Where(d => d.CaseStatus == "جارية" && !d.IsDeleted)
            .ToListAsync();

        foreach (var dispute in disputes)
        {
            // جلسة قادمة خلال 7 أيام
            if (dispute.NextHearingDate.HasValue)
            {
                var daysLeft = (dispute.NextHearingDate.Value - now).Days;
                if (daysLeft >= 0 && daysLeft <= 7)
                {
                    bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                        .AnyAsync(a => a.EntityType == "LegalDispute"
                                    && a.EntityId == dispute.Id
                                    && a.AlertType == "LegalHearing"
                                    && a.DueDate == dispute.NextHearingDate.Value.Date
                                    && !a.IsDeleted);
                    if (!exists)
                    {
                        await CreateAlertAsync(new AlertNotification
                        {
                            AlertType  = "LegalHearing",
                            Severity   = daysLeft <= 2 ? "حرج" : "تحذير",
                            Title      = "جلسة قضائية قريبة",
                            Message    = $"جلسة القضية [{dispute.CaseNumber}] في محكمة {dispute.CourtName} بعد {daysLeft} يوم",
                            EntityType = "LegalDispute",
                            EntityId   = dispute.Id,
                            EntityName = dispute.CaseNumber,
                            ActionUrl  = $"/Disputes/Details/{dispute.Id}",
                            DueDate    = dispute.NextHearingDate,
                            DaysLeft   = daysLeft
                        });
                    }
                }
            }

            // دعوى جارية لأكثر من سنة بدون تحديث
            if (dispute.UpdatedAt.HasValue && (now - dispute.UpdatedAt.Value).Days > 365)
            {
                bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                    .AnyAsync(a => a.EntityType == "LegalDispute"
                                && a.EntityId == dispute.Id
                                && a.AlertType == "LegalStagnant"
                                && a.CreatedAt > DateTime.Now.AddDays(-30)
                                && !a.IsDeleted);
                if (!exists)
                {
                    await CreateAlertAsync(new AlertNotification
                    {
                        AlertType  = "LegalStagnant",
                        Severity   = "تحذير",
                        Title      = "دعوى قضائية راكدة",
                        Message    = $"القضية [{dispute.CaseNumber}] لم تُحدَّث منذ أكثر من سنة — يرجى المراجعة",
                        EntityType = "LegalDispute",
                        EntityId   = dispute.Id,
                        EntityName = dispute.CaseNumber,
                        ActionUrl  = $"/Disputes/Details/{dispute.Id}",
                        DaysLeft   = 0
                    });
                }
            }
        }
    }

    // ---- تنبيهات الصيانة المستحقة ----
    public async Task GenerateMaintenanceDueAlertsAsync()
    {
        var now = DateTime.Today;
        var dueMaintenance = await _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Where(m => (m.Status == "مجدولة" || m.Status == "متأخرة")
                     && m.ScheduledDate <= now.AddDays(7)
                     && !m.IsDeleted)
            .ToListAsync();

        foreach (var maint in dueMaintenance)
        {
            var daysLeft = (maint.ScheduledDate - now).Days;
            bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                .AnyAsync(a => a.EntityType == "MaintenanceRecord"
                            && a.EntityId == maint.Id
                            && a.AlertType == "MaintenanceDue"
                            && a.CreatedAt.Date == now
                            && !a.IsDeleted);
            if (exists) continue;

            await CreateAlertAsync(new AlertNotification
            {
                AlertType  = "MaintenanceDue",
                Severity   = daysLeft < 0 ? "حرج" : (daysLeft <= 2 ? "تحذير" : "تنبيه"),
                Title      = daysLeft < 0 ? "صيانة متأخرة" : "صيانة مجدولة قريباً",
                Message    = daysLeft < 0
                    ? $"صيانة [{maint.Title}] لـ{maint.EntityName} متأخرة {Math.Abs(daysLeft)} يوم"
                    : $"صيانة [{maint.Title}] لـ{maint.EntityName} مجدولة بعد {daysLeft} يوم ({maint.ScheduledDate:dd/MM/yyyy})",
                EntityType = "MaintenanceRecord",
                EntityId   = maint.Id,
                EntityName = maint.EntityName,
                ActionUrl  = $"/Maintenance/Details/{maint.Id}",
                DueDate    = maint.ScheduledDate,
                DaysLeft   = daysLeft,
                ProvinceId = maint.ProvinceId
            });
        }
    }

    // ---- تنبيهات التجاوزات غير المعالجة ----
    public async Task GenerateEncroachmentAlertsAsync()
    {
        var now = DateTime.Today;
        var staleEncroachments = await _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Where(e => e.Status == "قائم"
                     && e.DiscoveryDate <= now.AddMonths(-1)
                     && !e.IsDeleted)
            .ToListAsync();

        foreach (var enc in staleEncroachments)
        {
            var daysOld = (now - enc.DiscoveryDate).Days;
            bool exists = await _unitOfWork.Repository<AlertNotification>().Query()
                .AnyAsync(a => a.EntityType == "EncroachmentRecord"
                            && a.EntityId == enc.Id
                            && a.AlertType == "EncroachmentUnresolved"
                            && a.CreatedAt > DateTime.Now.AddDays(-7)
                            && !a.IsDeleted);
            if (exists) continue;

            await CreateAlertAsync(new AlertNotification
            {
                AlertType  = "EncroachmentUnresolved",
                Severity   = daysOld > 90 ? "حرج" : "تحذير",
                Title      = "تجاوز غير معالج",
                Message    = $"التجاوز على [{enc.EntityName}] قائم منذ {daysOld} يوم دون إجراء",
                EntityType = "EncroachmentRecord",
                EntityId   = enc.Id,
                EntityName = enc.EntityName,
                ActionUrl  = $"/Encroachements/Details/{enc.Id}",
                DueDate    = enc.DiscoveryDate,
                DaysLeft   = -daysOld,
                ProvinceId = enc.ProvinceId
            });
        }
    }

    // =================== قراءة التنبيهات ===================

    public async Task<List<AlertNotification>> GetActiveAlertsAsync(int? provinceId = null, int limit = 50)
    {
        var q = _unitOfWork.Repository<AlertNotification>().Query()
            .Where(a => !a.IsDismissed && !a.IsDeleted);

        if (provinceId.HasValue)
            q = q.Where(a => a.ProvinceId == null || a.ProvinceId == provinceId.Value);

        return await q
            .OrderBy(a => a.IsRead)
            .ThenByDescending(a => a.Severity == "حرج" ? 4 : a.Severity == "تحذير" ? 3 : a.Severity == "تنبيه" ? 2 : 1)
            .ThenByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<AlertSummary> GetAlertSummaryAsync(int? provinceId = null)
    {
        var q = _unitOfWork.Repository<AlertNotification>().Query()
            .Where(a => !a.IsDismissed && !a.IsDeleted);

        if (provinceId.HasValue)
            q = q.Where(a => a.ProvinceId == null || a.ProvinceId == provinceId.Value);

        var alerts = await q.ToListAsync();

        return new AlertSummary
        {
            TotalUnread    = alerts.Count(a => !a.IsRead),
            CriticalCount  = alerts.Count(a => a.Severity == "حرج"   && !a.IsRead),
            WarningCount   = alerts.Count(a => a.Severity == "تحذير"  && !a.IsRead),
            InfoCount      = alerts.Count(a => a.Severity == "تنبيه" && !a.IsRead),
            ContractAlerts    = alerts.Count(a => (a.AlertType == "ContractExpiry" || a.AlertType == "ContractExpired") && !a.IsRead),
            PaymentAlerts     = alerts.Count(a => a.AlertType == "LatePayment"          && !a.IsRead),
            LegalAlerts       = alerts.Count(a => (a.AlertType == "LegalHearing" || a.AlertType == "LegalStagnant") && !a.IsRead),
            MaintenanceAlerts = alerts.Count(a => a.AlertType == "MaintenanceDue"       && !a.IsRead),
            EncroachAlerts    = alerts.Count(a => a.AlertType == "EncroachmentUnresolved" && !a.IsRead),
        };
    }

    public async Task MarkAsReadAsync(int alertId, string userId)
    {
        var alert = await _unitOfWork.Repository<AlertNotification>().GetByIdAsync(alertId);
        if (alert == null) return;
        alert.IsRead = true;
        alert.ReadAt = DateTime.Now;
        alert.ReadBy = userId;
        await _unitOfWork.Repository<AlertNotification>().UpdateAsync(alert);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int? provinceId, string userId)
    {
        var q = _unitOfWork.Repository<AlertNotification>().Query()
            .Where(a => !a.IsRead && !a.IsDismissed && !a.IsDeleted);
        if (provinceId.HasValue)
            q = q.Where(a => a.ProvinceId == null || a.ProvinceId == provinceId.Value);

        var alerts = await q.ToListAsync();
        foreach (var a in alerts) { a.IsRead = true; a.ReadAt = DateTime.Now; a.ReadBy = userId; }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DismissAlertAsync(int alertId)
    {
        var alert = await _unitOfWork.Repository<AlertNotification>().GetByIdAsync(alertId);
        if (alert == null) return;
        alert.IsDismissed = true;
        await _unitOfWork.Repository<AlertNotification>().UpdateAsync(alert);
        await _unitOfWork.SaveChangesAsync();
    }

    // =================== Helper ===================
    private async Task CreateAlertAsync(AlertNotification alert)
    {
        await _unitOfWork.Repository<AlertNotification>().AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class AlertSummary
{
    public int TotalUnread       { get; set; }
    public int CriticalCount     { get; set; }
    public int WarningCount      { get; set; }
    public int InfoCount         { get; set; }
    public int ContractAlerts    { get; set; }
    public int PaymentAlerts     { get; set; }
    public int LegalAlerts       { get; set; }
    public int MaintenanceAlerts { get; set; }
    public int EncroachAlerts    { get; set; }
}
