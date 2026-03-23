using System;
using FluentValidation;
using WaqfSystem.Application.DTOs.Document;

namespace WaqfSystem.Application.Validators
{
    public class UploadDocumentValidator : AbstractValidator<UploadDocumentDto>
    {
        public UploadDocumentValidator()
        {
            RuleFor(x => x.PropertyId)
                .GreaterThan(0)
                .WithMessage("يجب تحديد العقار");

            RuleFor(x => x.DocumentTypeId)
                .GreaterThan(0)
                .WithMessage("يجب اختيار نوع الوثيقة");

            RuleFor(x => x.Title)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(300)
                .WithMessage("عنوان الوثيقة مطلوب (3 أحرف على الأقل)");

            RuleFor(x => x.ExpiryDate)
                .Must((dto, date) =>
                {
                    if (date == null) return true;
                    return date.Value.Date > DateTime.Today;
                })
                .WithMessage("تاريخ انتهاء الصلاحية يجب أن يكون في المستقبل");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("الملف مطلوب");
        }
    }

    public class VerifyDocumentValidator : AbstractValidator<VerifyDocumentDto>
    {
        public VerifyDocumentValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0)
                .WithMessage("معرّف الوثيقة غير صالح");

            RuleFor(x => x)
                .Must(dto => dto.IsApproved || !string.IsNullOrWhiteSpace(dto.Notes))
                .WithMessage("سبب الرفض مطلوب عند رفض الوثيقة");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("الملاحظات يجب ألا تتجاوز 1000 حرف");
        }
    }

    public class UploadNewVersionValidator : AbstractValidator<UploadNewVersionDto>
    {
        public UploadNewVersionValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0)
                .WithMessage("معرّف الوثيقة غير صالح");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("ملف النسخة الجديدة مطلوب");
        }
    }
}
