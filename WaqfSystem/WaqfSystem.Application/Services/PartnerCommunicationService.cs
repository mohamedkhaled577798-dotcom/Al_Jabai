using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class PartnerCommunicationService : IPartnerCommunicationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;
        private readonly ILogger<PartnerCommunicationService> _logger;

        public PartnerCommunicationService(
            IEmailService emailService,
            ISmsService smsService,
            IWhatsAppService whatsAppService,
            INotificationService notificationService,
            IReportService reportService,
            ILogger<PartnerCommunicationService> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _whatsAppService = whatsAppService;
            _notificationService = notificationService;
            _reportService = reportService;
            _logger = logger;
        }

        public string BuildDistributionEmailHtml(PartnerRevenueDistribution dist, PropertyPartnership p)
        {
            var propertyName = p.Property?.PropertyName ?? p.Property?.WqfNumber ?? "-";
            var wqfNumber = p.Property?.WqfNumber ?? "-";
            return $@"<!doctype html>
<html lang='ar'>
<head><meta charset='utf-8'/></head>
<body style='direction:rtl;font-family:Arial;background:#f8faf8;padding:20px;'>
<div style='max-width:700px;margin:0 auto;background:#fff;border:1px solid #dfe7df;border-radius:10px;overflow:hidden;'>
<div style='background:#0f6f3e;color:#fff;padding:14px 20px;font-size:20px;font-weight:bold;'>هيئة الوقف السني — كشف توزيع الإيراد</div>
<div style='padding:20px;'>
<p>العقار: <strong>{propertyName}</strong></p>
<p>رقم الوقف: <strong>{wqfNumber}</strong></p>
<p>الفترة: {dist.PeriodStartDate:yyyy/MM/dd} - {dist.PeriodEndDate:yyyy/MM/dd}</p>
<table style='width:100%;border-collapse:collapse;margin-top:16px;'>
<tr style='background:#f0f6f0;'>
<th style='border:1px solid #ccd8cc;padding:10px;'>إجمالي الإيراد</th>
<th style='border:1px solid #ccd8cc;padding:10px;'>حصة الوقف</th>
<th style='border:1px solid #ccd8cc;padding:10px;'>حصتك</th>
</tr>
<tr>
<td style='border:1px solid #ccd8cc;padding:10px;text-align:center;'>{dist.TotalRevenue.ToString("N0")} د.ع</td>
<td style='border:1px solid #ccd8cc;padding:10px;text-align:center;'>{dist.WaqfAmount.ToString("N0")} د.ع</td>
<td style='border:1px solid #ccd8cc;padding:10px;text-align:center;'>{dist.PartnerAmount.ToString("N0")} د.ع</td>
</tr>
</table>
<p style='margin-top:18px;color:#5f6661;'>تاريخ الإصدار: {DateTime.Now:yyyy/MM/dd HH:mm}</p>
<p style='color:#5f6661;'>للاستفسار يرجى التواصل مع قسم العقود والاستثمار.</p>
</div>
</div>
</body>
</html>";
        }

        public string BuildExpiryWarningEmailHtml(PropertyPartnership p, int daysRemaining)
        {
            var urgent = daysRemaining <= 30;
            var color = urgent ? "#9b1c1c" : "#b45309";
            var bg = urgent ? "#fff1f1" : "#fff8eb";
            var title = urgent ? "تنبيه عاجل بانتهاء الشراكة" : "تنبيه بقرب انتهاء الشراكة";
            return $@"<!doctype html>
<html lang='ar'>
<head><meta charset='utf-8'/></head>
<body style='direction:rtl;font-family:Arial;background:{bg};padding:20px;'>
<div style='max-width:680px;margin:0 auto;background:#fff;border:1px solid #e6e2d5;border-radius:10px;'>
<div style='padding:14px 18px;background:{color};color:#fff;font-size:19px;font-weight:bold;'>{title}</div>
<div style='padding:20px;'>
<p>السيد/السيدة {p.PartnerName} المحترم،</p>
<p>نود إعلامكم بأن الشراكة الخاصة بالعقار رقم <strong>{p.Property?.WqfNumber ?? "-"}</strong> تنتهي بعد <strong>{daysRemaining}</strong> يوم.</p>
<p>يرجى مراجعة إدارة العقود لاستكمال الإجراءات المطلوبة.</p>
<p style='margin-top:20px;color:#555;'>هيئة إدارة واستثمار أموال الوقف السني</p>
</div>
</div>
</body>
</html>";
        }

        public string BuildTransferConfirmationEmailHtml(PartnerRevenueDistribution dist, PropertyPartnership p)
        {
            return $@"<!doctype html>
<html lang='ar'>
<head><meta charset='utf-8'/></head>
<body style='direction:rtl;font-family:Arial;background:#f8faf8;padding:20px;'>
<div style='max-width:680px;margin:0 auto;background:#fff;border:1px solid #dfe7df;border-radius:10px;'>
<div style='padding:14px 18px;background:#0f6f3e;color:#fff;font-size:19px;font-weight:bold;'>تأكيد تحويل مستحقات الشريك</div>
<div style='padding:20px;'>
<p>تم تحويل مستحقاتكم بمبلغ <strong>{dist.PartnerAmount.ToString("N0")} د.ع</strong>.</p>
<p>طريقة التحويل: {dist.TransferMethod ?? "غير محدد"}</p>
<p>رقم المرجع: <strong>{dist.TransferReference ?? "-"}</strong></p>
<p>العقار: {p.Property?.PropertyName ?? p.Property?.WqfNumber ?? "-"}</p>
<p style='color:#666;'>شاكرين تعاونكم.</p>
</div>
</div>
</body>
</html>";
        }

        public string BuildSmsText(string eventType, PropertyPartnership p, decimal? amount = null)
        {
            return eventType switch
            {
                "DistributionCreated" => $"تم تسجيل توزيع جديد لعقار {p.Property?.WqfNumber}. حصتك: {(amount ?? 0).ToString("N0")} د.ع",
                "Expiry90" => $"تنبيه: شراكتكم للعقار {p.Property?.WqfNumber} تنتهي خلال 90 يوم.",
                "Expiry30" => $"عاجل: شراكتكم للعقار {p.Property?.WqfNumber} تنتهي خلال 30 يوم. يرجى المراجعة.",
                "Transferred" => $"تم تحويل مستحقاتكم لعقار {p.Property?.WqfNumber}.",
                _ => $"إشعار من هيئة الوقف السني بخصوص العقار {p.Property?.WqfNumber}."
            };
        }

        public async Task<bool> SendDistributionNotificationAsync(PartnerRevenueDistribution dist, PropertyPartnership p, int userId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(p.PartnerEmail))
                {
                    await _emailService.SendAsync(p.PartnerEmail, "كشف توزيع الإيراد", BuildDistributionEmailHtml(dist, p));
                }
                if (!string.IsNullOrWhiteSpace(p.PartnerPhone))
                {
                    await _smsService.SendAsync(p.PartnerPhone, BuildSmsText("DistributionCreated", p, dist.PartnerAmount));
                }
                if (!string.IsNullOrWhiteSpace(p.PartnerWhatsApp))
                {
                    await _whatsAppService.SendAsync(p.PartnerWhatsApp, BuildSmsText("DistributionCreated", p, dist.PartnerAmount));
                }

                await _notificationService.SendToRoleAsync("CONTRACTS_MGR", "توزيع إيراد", $"تم تسجيل توزيع جديد للشراكة {p.Id}", "PropertyPartnerships", p.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendDistributionNotificationAsync failed");
                return false;
            }
        }

        public async Task<bool> SendExpiryWarningAsync(PropertyPartnership p, int daysRemaining, int triggeredByUserId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(p.PartnerEmail))
                {
                    await _emailService.SendAsync(p.PartnerEmail, "تنبيه انتهاء الشراكة", BuildExpiryWarningEmailHtml(p, daysRemaining));
                }

                if (daysRemaining <= 30)
                {
                    if (!string.IsNullOrWhiteSpace(p.PartnerPhone))
                    {
                        await _smsService.SendAsync(p.PartnerPhone, BuildSmsText("Expiry30", p));
                    }
                    if (!string.IsNullOrWhiteSpace(p.PartnerWhatsApp))
                    {
                        await _whatsAppService.SendAsync(p.PartnerWhatsApp, BuildSmsText("Expiry30", p));
                    }
                }

                await _notificationService.SendToRoleAsync("REGIONAL_MGR", "تنبيه انتهاء شراكة", $"الشراكة رقم {p.Id} تنتهي خلال {daysRemaining} يوم", "PropertyPartnerships", p.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendExpiryWarningAsync failed");
                return false;
            }
        }

        public async Task<bool> SendTransferConfirmationAsync(PartnerRevenueDistribution dist, PropertyPartnership p, int userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(p.PartnerEmail))
                {
                    return true;
                }

                return await _emailService.SendAsync(p.PartnerEmail, "تأكيد تحويل مستحقات", BuildTransferConfirmationEmailHtml(dist, p));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendTransferConfirmationAsync failed");
                return false;
            }
        }

        public async Task<bool> SendManualMessageAsync(SendCommunicationDto dto, PropertyPartnership p, int userId)
        {
            try
            {
                switch (dto.ContactType)
                {
                    case ContactType.SMS:
                        if (string.IsNullOrWhiteSpace(p.PartnerPhone)) return false;
                        return (await _smsService.SendAsync(p.PartnerPhone, dto.MessageBody)) != null;

                    case ContactType.WhatsApp:
                        if (string.IsNullOrWhiteSpace(p.PartnerWhatsApp)) return false;
                        return (await _whatsAppService.SendAsync(p.PartnerWhatsApp, dto.MessageBody)) != null;

                    case ContactType.Email:
                        if (string.IsNullOrWhiteSpace(p.PartnerEmail)) return false;
                        return await _emailService.SendAsync(p.PartnerEmail, dto.Subject ?? "رسالة", $"<div style='direction:rtl;font-family:Arial'>{dto.MessageBody}</div>");

                    case ContactType.PDF:
                        if (string.IsNullOrWhiteSpace(p.PartnerEmail)) return false;
                        var pdf = await _reportService.GeneratePartnerStatementAsync(dto.PartnershipId);
                        return await _emailService.SendWithAttachmentAsync(
                            p.PartnerEmail,
                            dto.Subject ?? "كشف حساب الشريك",
                            $"<div style='direction:rtl;font-family:Arial'>{dto.MessageBody}</div>",
                            pdf,
                            $"statement_{dto.PartnershipId}_{DateTime.Now:yyyyMMdd}.pdf");

                    case ContactType.Phone:
                    case ContactType.Meeting:
                    case ContactType.Letter:
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendManualMessageAsync failed");
                return false;
            }
        }
    }
}
