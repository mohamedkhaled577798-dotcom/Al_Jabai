using FluentValidation;
using WaqfSystem.Application.DTOs.Property;

namespace WaqfSystem.Application.Validators
{
    public class CreatePropertyValidator : AbstractValidator<CreatePropertyDto>
    {
        public CreatePropertyValidator()
        {
            RuleFor(x => x.PropertyName)
                .NotEmpty().WithMessage("اسم العقار مطلوب")
                .MaximumLength(300).WithMessage("اسم العقار يجب ألا يتجاوز 300 حرف");

            RuleFor(x => x.GovernorateId)
                .NotNull().WithMessage("المحافظة مطلوبة");

            RuleFor(x => x.PropertyType)
                .IsInEnum().WithMessage("نوع العقار غير صالح");

            RuleFor(x => x.TotalAreaSqm)
                .GreaterThan(0).When(x => x.TotalAreaSqm.HasValue)
                .WithMessage("المساحة يجب أن تكون أكبر من صفر");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
                .WithMessage("خط العرض غير صالح");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
                .WithMessage("خط الطول غير صالح");
        }
    }

    public class LegacyCreatePartnershipValidator : AbstractValidator<CreatePartnershipDto>
    {
        public LegacyCreatePartnershipValidator()
        {
            RuleFor(x => x.PartnerName)
                .NotEmpty().WithMessage("اسم الشريك مطلوب");

            RuleFor(x => x.PartnerSharePercent)
                .InclusiveBetween(0.01m, 99.99m)
                .WithMessage("نسبة الشراكة يجب أن تكون بين 0.01 و 99.99");
        }
    }

    public class UploadDocumentValidator : AbstractValidator<UploadDocumentDto>
    {
        public UploadDocumentValidator()
        {
            RuleFor(x => x.DocumentCategory)
                .IsInEnum().WithMessage("فئة المستند غير صالحة");

            RuleFor(x => x.FileFormat)
                .IsInEnum().WithMessage("صيغة الملف غير صالحة");
        }
    }
}
