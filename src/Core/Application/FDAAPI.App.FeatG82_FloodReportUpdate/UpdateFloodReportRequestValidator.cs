using FluentValidation;
using System;

namespace FDAAPI.App.FeatG82_FloodReportUpdate
{
    public class UpdateFloodReportRequestValidator : AbstractValidator<UpdateFloodReportRequest>
    {
        public UpdateFloodReportRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Report ID is required");

            RuleFor(x => x.UserId)
                .NotNull()
                .WithMessage("User ID is required");

            RuleFor(x => x.UserRole)
                .NotEmpty()
                .WithMessage("User role is required");

            // Address validation (optional field)
            When(x => !string.IsNullOrEmpty(x.Address), () =>
            {
                RuleFor(x => x.Address)
                    .MinimumLength(3)
                    .WithMessage("Address must be at least 3 characters")
                    .MaximumLength(500)
                    .WithMessage("Address must not exceed 500 characters")
                    .Matches(@"[a-zA-Z0-9\s,.\-#]")
                    .WithMessage("Address contains invalid characters");
            });

            // Description validation (optional field)
            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MinimumLength(5)
                    .WithMessage("Description must be at least 5 characters")
                    .MaximumLength(1000)
                    .WithMessage("Description must not exceed 1000 characters");
                    // Note: XSS sanitization done in handler, not validator
            });

            // Severity validation (optional field)
            When(x => !string.IsNullOrEmpty(x.Severity), () =>
            {
                RuleFor(x => x.Severity)
                    .Must(s => new[] { "low", "medium", "high" }
                        .Contains(s.ToLower()))
                    .WithMessage("Severity must be one of: low, medium, high");
            });
        }
    }
}
