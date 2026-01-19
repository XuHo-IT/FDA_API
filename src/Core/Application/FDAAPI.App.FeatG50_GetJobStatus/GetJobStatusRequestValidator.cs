using FluentValidation;

namespace FDAAPI.App.FeatG50_GetJobStatus
{
    public class GetJobStatusRequestValidator : AbstractValidator<GetJobStatusRequest>
    {
        public GetJobStatusRequestValidator()
        {
            RuleFor(x => x.JobRunId)
                .NotEmpty().WithMessage("Job run ID is required.");
        }
    }
}

