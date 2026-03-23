using System;
using FluentValidation;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Validators
{
    public class QuickCollectDtoValidator : AbstractValidator<QuickCollectDto>
    {
        public QuickCollectDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("المبلغ يجب أن يكون أكبر من صفر");

            RuleFor(x => x.CollectionDate)
                .LessThanOrEqualTo(_ => DateTime.Today.AddDays(1))
                .WithMessage("تاريخ التحصيل لا يمكن في المستقبل");

            RuleFor(x => x.FloorId)
                .NotNull()
                .When(x => x.CollectionLevel == CollectionLevel.Floor)
                .WithMessage("حدد الطابق");

            RuleFor(x => x.UnitId)
                .NotNull()
                .When(x => x.CollectionLevel == CollectionLevel.Unit)
                .WithMessage("حدد الوحدة");

            RuleFor(x => x)
                .Custom((dto, context) =>
                {
                    if (!dto.ExpectedAmount.HasValue || dto.ExpectedAmount.Value <= 0)
                    {
                        return;
                    }

                    var pct = Math.Abs((dto.Amount - dto.ExpectedAmount.Value) / dto.ExpectedAmount.Value);
                    if (pct > 0.10m && string.IsNullOrWhiteSpace(dto.VarianceApprovalNote))
                    {
                        context.AddFailure("VarianceApprovalNote", "الفارق أكبر من 10% — أضف ملاحظة موافقة");
                    }
                });
        }
    }

    public class BatchCollectDtoValidator : AbstractValidator<BatchCollectDto>
    {
        public BatchCollectDtoValidator()
        {
            RuleFor(x => x.Items)
                .NotNull()
                .Must(x => x != null && x.Count > 0)
                .WithMessage("يجب تحديد وحدة واحدة على الأقل");

            RuleFor(x => x.Items)
                .Must(x => x != null && x.Count <= 50)
                .WithMessage("الدفعة الواحدة لا تتجاوز 50 عنصراً");

            RuleForEach(x => x.Items)
                .SetValidator(new BatchCollectItemDtoValidator());
        }
    }

    public class BatchCollectItemDtoValidator : AbstractValidator<BatchCollectItemDto>
    {
        public BatchCollectItemDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("المبلغ يجب أن يكون أكبر من صفر");
        }
    }
}
