using FluentValidation;

namespace FDAAPI.App.FeatG102_AlertTemplateDelete
{
    public class DeleteAlertTemplateRequestValidator : AbstractValidator<DeleteAlertTemplateRequest>
    {
        public DeleteAlertTemplateRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
