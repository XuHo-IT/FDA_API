using FluentValidation;

namespace FDAAPI.App.FeatG14_ProfileGet
{
    public class GetProfileRequestValidator : AbstractValidator<GetProfileRequest>
    {
        public GetProfileRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}

