using System;
using FluentValidation;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.Validators
{
    public class CreateMissionDtoValidator : AbstractValidator<CreateMissionDto>
    {
        public CreateMissionDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان المهمة مطلوب")
                .MinimumLength(5).WithMessage("عنوان المهمة يجب ألا يقل عن 5 أحرف")
                .MaximumLength(200).WithMessage("عنوان المهمة يجب ألا يزيد عن 200 حرف");

            RuleFor(x => x.MissionDate)
                .NotEmpty().WithMessage("تاريخ المهمة مطلوب")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("تاريخ المهمة يجب أن يكون اليوم أو بعده");

            RuleFor(x => x.GovernorateId)
                .GreaterThan(0).WithMessage("المحافظة مطلوبة");

            RuleFor(x => x.TargetPropertyCount)
                .InclusiveBetween(0, 99999).WithMessage("عدد العقارات المستهدفة يجب أن يكون بين 0 و 99999");

            RuleFor(x => x.ExpectedCompletionDate)
                .GreaterThan(x => x.MissionDate)
                .When(x => x.ExpectedCompletionDate.HasValue)
                .WithMessage("تاريخ الإنجاز المتوقع يجب أن يكون بعد تاريخ المهمة");
        }
    }

    public class AssignMissionDtoValidator : AbstractValidator<AssignMissionDto>
    {
        public AssignMissionDtoValidator()
        {
            RuleFor(x => x.MissionId)
                .GreaterThan(0).WithMessage("رقم المهمة غير صحيح");

            RuleFor(x => x.AssignedToUserId)
                .GreaterThan(0).WithMessage("يجب اختيار موظف للإسناد");
        }
    }

    public class AdvanceStageDtoValidator : AbstractValidator<AdvanceStageDto>
    {
        public AdvanceStageDtoValidator()
        {
            RuleFor(x => x.MissionId)
                .GreaterThan(0).WithMessage("رقم المهمة غير صحيح");

            RuleFor(x => x)
                .Must(x => IsAllowedTransition(x.ToStage))
                .WithMessage(x => $"الانتقال إلى {x.ToStage} غير مسموح به");
        }

        private static bool IsAllowedTransition(MissionStage to)
        {
            return Enum.IsDefined(typeof(MissionStage), to);
        }
    }
}
