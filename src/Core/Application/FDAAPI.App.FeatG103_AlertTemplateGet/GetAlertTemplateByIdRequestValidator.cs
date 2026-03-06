using FluentValidation;

namespace FDAAPI.App.FeatG103_AlertTemplateGet
{
    public class GetAlertTemplateByIdRequestValidator : AbstractValidator<GetAlertTemplateByIdRequest>
    {
        public GetAlertTemplateByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
