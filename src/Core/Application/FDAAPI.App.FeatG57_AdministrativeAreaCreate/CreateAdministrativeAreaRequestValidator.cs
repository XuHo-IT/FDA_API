using FluentValidation;

namespace FDAAPI.App.FeatG57_AdministrativeAreaCreate
{
    public class CreateAdministrativeAreaRequestValidator : AbstractValidator<CreateAdministrativeAreaRequest>
    {
        public CreateAdministrativeAreaRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

            RuleFor(x => x.Level)
                .NotEmpty().WithMessage("Level is required.")
                .Must(level => level == "city" || level == "district" || level == "ward")
                .WithMessage("Level must be one of: city, district, ward.");

            RuleFor(x => x.Code)
                .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.Code));

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required.");
        }
    }
}

