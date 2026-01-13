using FluentValidation;

namespace FDAAPI.App.FeatG34_AreaStatusEvaluate
{
    public class AreaStatusEvaluateRequestValidator : AbstractValidator<AreaStatusEvaluateRequest>
    {
        public AreaStatusEvaluateRequestValidator()
        {
            RuleFor(x => x.AreaId)
                .NotEmpty().WithMessage("Area ID is required.");
        }
    }
}

