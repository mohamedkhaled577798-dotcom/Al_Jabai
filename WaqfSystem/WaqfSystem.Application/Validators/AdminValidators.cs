using System.Text.RegularExpressions;
using FluentValidation;
using WaqfSystem.Application.DTOs.Admin;

namespace WaqfSystem.Application.Validators
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الدور مطلوب")
                .Matches("^[A-Z_]+$").WithMessage("اسم الدور يجب أن يكون بحروف إنجليزية كبيرة وشرطة سفلية فقط");

            RuleFor(x => x.DisplayNameAr)
                .NotEmpty().WithMessage("الاسم العربي للدور مطلوب")
                .MinimumLength(2).WithMessage("الاسم العربي يجب ألا يقل عن حرفين")
                .MaximumLength(100).WithMessage("الاسم العربي يجب ألا يزيد عن 100 حرف");

            RuleFor(x => x.Color)
                .Must(x => string.IsNullOrWhiteSpace(x) || Regex.IsMatch(x, "^#[0-9A-Fa-f]{6}$"))
                .WithMessage("لون غير صالح - يجب أن يكون بصيغة #RRGGBB");
        }
    }

    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FullNameAr)
                .NotEmpty().WithMessage("الاسم الكامل باللغة العربية مطلوب (3 أحرف على الأقل)")
                .MinimumLength(3).WithMessage("الاسم الكامل باللغة العربية مطلوب (3 أحرف على الأقل)");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
                .Must(p => p.Any(char.IsUpper) && p.Any(char.IsDigit))
                .WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير ورقم على الأقل");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("كلمة المرور وتأكيدها غير متطابقتين");

            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("يجب اختيار دور للمستخدم");

            RuleFor(x => x.DistrictId)
                .NotNull()
                .When(x => x.SubDistrictId.HasValue)
                .WithMessage("يجب اختيار القضاء قبل الناحية");

            RuleFor(x => x.GovernorateId)
                .NotNull()
                .When(x => x.DistrictId.HasValue || x.SubDistrictId.HasValue)
                .WithMessage("يجب اختيار المحافظة قبل النطاق الأدنى");
        }
    }

    public class CreateGovernorateValidator : AbstractValidator<CreateGovernorateDto>
    {
        public CreateGovernorateValidator()
        {
            RuleFor(x => x.NameAr).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(10)
                .Matches("^[A-Z]+$").WithMessage("الكود يجب أن يكون بحروف إنجليزية كبيرة فقط (مثال: BGH)");
        }
    }

    public class CreateDistrictValidator : AbstractValidator<CreateDistrictDto>
    {
        public CreateDistrictValidator()
        {
            RuleFor(x => x.GovernorateId).GreaterThan(0).WithMessage("يجب اختيار المحافظة");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم القضاء مطلوب");
            RuleFor(x => x.Code)
                .Must(c => string.IsNullOrWhiteSpace(c) || Regex.IsMatch(c, "^[A-Z0-9_]+$"))
                .WithMessage("كود القضاء غير صالح");
        }
    }

    public class CreateSubDistrictValidator : AbstractValidator<CreateSubDistrictDto>
    {
        public CreateSubDistrictValidator()
        {
            RuleFor(x => x.DistrictId).GreaterThan(0).WithMessage("يجب اختيار القضاء");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم الناحية مطلوب");
            RuleFor(x => x.Code)
                .Must(c => string.IsNullOrWhiteSpace(c) || Regex.IsMatch(c, "^[A-Z0-9_]+$"))
                .WithMessage("كود الناحية غير صالح");
        }
    }

    public class CreateNeighborhoodValidator : AbstractValidator<CreateNeighborhoodDto>
    {
        public CreateNeighborhoodValidator()
        {
            RuleFor(x => x.SubDistrictId).GreaterThan(0).WithMessage("يجب اختيار الناحية");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم الحي مطلوب");
            RuleFor(x => x.Code)
                .Must(c => string.IsNullOrWhiteSpace(c) || Regex.IsMatch(c, "^[A-Z0-9_]+$"))
                .WithMessage("كود الحي غير صالح");
        }
    }

    public class CreateStreetValidator : AbstractValidator<CreateStreetDto>
    {
        public CreateStreetValidator()
        {
            RuleFor(x => x.NeighborhoodId).GreaterThan(0).WithMessage("يجب اختيار الحي");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم الشارع مطلوب");
            RuleFor(x => x.Code)
                .Must(c => string.IsNullOrWhiteSpace(c) || Regex.IsMatch(c, "^[A-Z0-9_]+$"))
                .WithMessage("كود الشارع غير صالح");
        }
    }
}
