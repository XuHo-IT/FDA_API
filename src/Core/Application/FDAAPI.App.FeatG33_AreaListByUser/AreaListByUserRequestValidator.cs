using FluentValidation;

namespace FDAAPI.App.FeatG33_AreaListByUser
{
    public class AreaListByUserRequestValidator : AbstractValidator<AreaListByUserRequest>
    {
        public AreaListByUserRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}
