using FluentValidation;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public class DeleteAreaRequestValidator : AbstractValidator<DeleteAreaRequest>
    {
        public DeleteAreaRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Area ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}

