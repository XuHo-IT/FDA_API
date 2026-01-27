using FluentValidation;

namespace FDAAPI.App.FeatG72_SubscribeToPlan
{
    public class SubscribeToPlanRequestValidator : AbstractValidator<SubscribeToPlanRequest>
    {
        public SubscribeToPlanRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");

            RuleFor(x => x.PlanCode)
                .NotEmpty()
                .WithMessage("PlanCode is required")
                .Must(code => new[] { "FREE", "PREMIUM", "MONITOR" }.Contains(code.ToUpper()))
                .WithMessage("PlanCode must be FREE, PREMIUM, or MONITOR");

            RuleFor(x => x.DurationMonths)
                .GreaterThan(0)
                .WithMessage("Duration must be at least 1 month")
                .LessThanOrEqualTo(120)
                .WithMessage("Duration cannot exceed 10 years");
        }
    }
}