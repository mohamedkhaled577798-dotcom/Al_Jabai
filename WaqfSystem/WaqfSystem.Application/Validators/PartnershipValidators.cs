using FluentValidation;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Validators
{
    public class CreatePartnershipValidator : AbstractValidator<CreatePartnershipDto>
    {
        public CreatePartnershipValidator()
        {
            RuleFor(x => x.WaqfSharePercent).InclusiveBetween(1, 99)
                .WithMessage("نسبة الوقف يجب أن تكون بين 1% و 99%");

            RuleFor(x => x.PartnerPhone).NotEmpty().Matches(@"^[\d\+\-\s]+$")
                .WithMessage("رقم الهاتف غير صالح");

            When(x => x.PartnerType == PartnerType.Individual || x.PartnerType == PartnerType.Heirs, () =>
            {
                RuleFor(x => x.PartnerNationalId).NotEmpty()
                    .WithMessage("رقم هوية الشريك مطلوب لهذا النوع");
            });

            When(x => x.PartnershipType == PartnershipType.FloorOwnership, () =>
            {
                RuleFor(x => x.OwnedFloorNumbers).NotNull().NotEmpty()
                    .WithMessage("يجب تحديد طوابق الوقف للشراكة من نوع ملكية طوابق");
            });

            When(x => x.PartnershipType == PartnershipType.UnitOwnership, () =>
            {
                RuleFor(x => x.OwnedUnitIds).NotNull().NotEmpty()
                    .WithMessage("يجب تحديد وحدات الوقف للشراكة من نوع ملكية عينية");
            });

            When(x => x.PartnershipType == PartnershipType.UsufructRight, () =>
            {
                RuleFor(x => x.UsufructStartDate).NotNull()
                    .WithMessage("تاريخ بدء حق الانتفاع مطلوب");
                RuleFor(x => x.UsufructEndDate).NotNull()
                    .GreaterThan(x => x.UsufructStartDate)
                    .WithMessage("تاريخ انتهاء حق الانتفاع يجب أن يكون بعد تاريخ البدء");
                RuleFor(x => x.UsufructAnnualFeePerYear).NotNull().GreaterThan(0)
                    .WithMessage("الرسوم السنوية لحق الانتفاع مطلوبة وتكون أكبر من صفر");
            });

            When(x => x.PartnershipType == PartnershipType.TimedPartnership, () =>
            {
                RuleFor(x => x.PartnershipStartDate).NotNull()
                    .WithMessage("تاريخ بدء الشراكة المؤقتة مطلوب");
                RuleFor(x => x.PartnershipEndDate).NotNull()
                    .GreaterThan(x => x.PartnershipStartDate)
                    .WithMessage("تاريخ انتهاء الشراكة المؤقتة يجب أن يكون بعد تاريخ البدء");
            });

            When(x => x.PartnershipType == PartnershipType.LandPercent, () =>
            {
                RuleFor(x => x.LandSharePercentWaqf).NotNull().InclusiveBetween(1, 99)
                    .WithMessage("نسبة الوقف من الأرض يجب أن تكون بين 1 و 99");
                RuleFor(x => x.LandTotalDunum).NotNull().GreaterThan(0)
                    .WithMessage("إجمالي مساحة الأرض مطلوب ويجب أن يكون أكبر من صفر");
            });

            When(x => x.PartnershipType == PartnershipType.HarvestShare, () =>
            {
                RuleFor(x => x.WaqfHarvestPercent).NotNull().InclusiveBetween(1, 99)
                    .WithMessage("نسبة الوقف من المحصول يجب أن تكون بين 1 و 99");
                RuleFor(x => x.FarmerName).NotEmpty()
                    .WithMessage("اسم المزارع مطلوب");
            });

            When(x => x.PartnershipType == PartnershipType.Custom, () =>
            {
                RuleFor(x => x.CustomPartnershipName).NotEmpty()
                    .WithMessage("اسم نوع الشراكة المخصصة مطلوب");
            });

            RuleForEach(x => x.ConditionRules).ChildRules(rule =>
            {
                rule.RuleFor(r => r.RuleName).NotEmpty().WithMessage("اسم الشرط مطلوب");
                rule.RuleFor(r => r.PriorityOrder).GreaterThanOrEqualTo(0);
            });

            When(x => x.RevenueDistribMethod == RevenueDistribMethod.Monthly, () =>
            {
                RuleFor(x => x.RevenueDistribDay).NotNull().InclusiveBetween(1, 28)
                    .WithMessage("يوم التوزيع مطلوب ويجب بين 1 و 28 للتوزيع الشهري");
            });
        }
    }

    public class UpdatePartnershipValidator : AbstractValidator<UpdatePartnershipDto>
    {
        public UpdatePartnershipValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف الشراكة غير صالح");

            When(x => x.WaqfSharePercent.HasValue, () =>
            {
                RuleFor(x => x.WaqfSharePercent!.Value).InclusiveBetween(1, 99)
                    .WithMessage("نسبة الوقف يجب أن تكون بين 1% و 99%");
            });

            When(x => x.RevenueDistribMethod == RevenueDistribMethod.Monthly, () =>
            {
                RuleFor(x => x.RevenueDistribDay).NotNull().InclusiveBetween(1, 28)
                    .WithMessage("يوم التوزيع مطلوب ويجب بين 1 و 28 للتوزيع الشهري");
            });
        }
    }
}
