using FluentValidation;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public class GetAreaRequestValidator : AbstractValidator<GetAreaRequest>
    {
        public GetAreaRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Area ID is required.");
        }
    }
}

