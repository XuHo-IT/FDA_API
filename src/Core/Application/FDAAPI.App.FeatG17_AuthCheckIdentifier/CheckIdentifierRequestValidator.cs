using FluentValidation;

namespace FDAAPI.App.FeatG17_AuthCheckIdentifier
{
    public class CheckIdentifierRequestValidator : AbstractValidator<CheckIdentifierRequest>
    {
        public CheckIdentifierRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier is required.")
                .MaximumLength(100).WithMessage("Identifier must not exceed 100 characters.");
        }
    }
}

