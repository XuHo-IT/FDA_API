using FluentValidation;

namespace FDAAPI.App.FeatG18_MediaUploadImage
{
    public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
    {
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

        public UploadImageRequestValidator()
        {
            RuleFor(x => x.ImageData)
                .NotNull().WithMessage("Image data is required.")
                .Must(data => data != null && data.Length > 0).WithMessage("Image data cannot be empty.")
                .Must(data => data.LongLength <= MaxBytes).WithMessage($"Image exceeds maximum size of {MaxBytes / (1024 * 1024)} MB.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .Must(fileName => 
                {
                    var ext = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
                    return AllowedExtensions.Contains(ext);
                }).WithMessage("Unsupported image extension. Allowed: .jpg, .jpeg, .png, .gif");

            RuleFor(x => x.Folder)
                .NotEmpty().WithMessage("Folder is required.")
                .MaximumLength(100).WithMessage("Folder path must not exceed 100 characters.");
        }
    }
}

