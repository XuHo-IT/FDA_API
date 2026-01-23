using FluentValidation;

namespace FDAAPI.App.FeatG55_AdministrativeAreasEvaluate
{
    public class AdministrativeAreasEvaluateRequestValidator : AbstractValidator<AdministrativeAreasEvaluateRequest>
    {
        public AdministrativeAreasEvaluateRequestValidator()
        {
            RuleFor(x => x.AdministrativeAreaId)
                .NotEmpty().WithMessage("Administrative Area ID is required.");
        }
    }
}

