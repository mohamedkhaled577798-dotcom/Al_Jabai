using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace WaqfSystem.Application.Services
{
    public class RevenueReportService : IRevenueReportService
    {
        private readonly IAppDbContext _db;

        public RevenueReportService(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> GenerateDailyCollectionReportAsync(DateTime date, int userId)
        {
            var revenues = await _db.PropertyRevenues
                .AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted && x.CollectionDate.Date == date.Date)
                .OrderBy(x => x.PropertyId)
                .ThenBy(x => x.UnitId)
                .ToListAsync();

            var pending = await _db.RentPaymentSchedules
                .AsNoTracking()
                .Where(x => !x.IsDeleted && !x.IsPaid && x.DueDate.Date <= date.Date)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("تقرير التحصيل اليومي");
            sb.AppendLine($"التاريخ: {date:yyyy/MM/dd}");
            sb.AppendLine("رمز التحصيل,العقار,الوحدة,المبلغ,المتوقع,الفارق");

            foreach (var r in revenues)
            {
                var expected = r.ExpectedAmount ?? 0;
                var variance = r.Amount - expected;
                sb.AppendLine($"{r.RevenueCode},{r.Property?.PropertyName},{r.Unit?.UnitNumber},{r.Amount:0.00},{expected:0.00},{variance:0.00}");
            }

            sb.AppendLine();
            sb.AppendLine($"إجمالي العمليات: {revenues.Count}");
            sb.AppendLine($"الإجمالي المحصل: {revenues.Sum(x => x.Amount):0.00}");
            sb.AppendLine($"إجمالي المستحقات غير المسددة: {pending.Sum(x => x.ExpectedAmount):0.00}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateBatchReceiptPdfAsync(string batchCode)
        {
            var batch = await _db.CollectionBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BatchCode == batchCode);

            if (batch == null)
            {
                throw new InvalidOperationException("الدفعة غير موجودة");
            }

            var revenues = await _db.PropertyRevenues
                .AsNoTracking()
                .Include(x => x.Property)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted && x.Batch.BatchCode == batchCode)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("إيصال دفعة تحصيل");
            sb.AppendLine($"رمز الدفعة: {batch.BatchCode}");
            sb.AppendLine($"الفترة: {batch.PeriodLabel}");
            sb.AppendLine($"تاريخ التحصيل: {batch.CollectionDate:yyyy/MM/dd}");
            sb.AppendLine("-------------------------------------------------");
            foreach (var r in revenues)
            {
                sb.AppendLine($"{r.Property?.PropertyName} / {r.Unit?.UnitNumber} : {r.Amount:0.00} د.ع");
            }
            sb.AppendLine("-------------------------------------------------");
            sb.AppendLine($"الإجمالي: {batch.TotalAmount:0.00} د.ع");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateMonthlyStatementAsync(long propertyId, string periodLabel)
        {
            var property = await _db.Properties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)propertyId);
            if (property == null)
            {
                throw new InvalidOperationException("العقار غير موجود");
            }

            var lines = await _db.PropertyRevenues
                .AsNoTracking()
                .Include(x => x.Floor)
                .Include(x => x.Unit)
                .Where(x => !x.IsDeleted && x.PropertyId == (int)propertyId && x.PeriodLabel == periodLabel)
                .OrderBy(x => x.FloorId)
                .ThenBy(x => x.UnitId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("كشف التحصيل الشهري");
            sb.AppendLine($"العقار: {property.PropertyName}");
            sb.AppendLine($"الفترة: {periodLabel}");
            sb.AppendLine("--------------------------------------------");

            var grouped = lines.GroupBy(x => x.Floor?.FloorLabel ?? "بدون طابق");
            foreach (var group in grouped)
            {
                sb.AppendLine($"{group.Key}");
                foreach (var row in group)
                {
                    sb.AppendLine($"  وحدة {row.Unit?.UnitNumber ?? "-"} : {row.Amount:0.00} د.ع");
                }

                sb.AppendLine($"  مجموع الطابق: {group.Sum(x => x.Amount):0.00} د.ع");
                sb.AppendLine();
            }

            sb.AppendLine($"الإجمالي العام: {lines.Sum(x => x.Amount):0.00} د.ع");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
