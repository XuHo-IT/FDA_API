using FluentValidation;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public class AreaGetRequestValidator : AbstractValidator<AreaGetRequest>
    {
        public AreaGetRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Area ID is required.");
        }
    }
}

