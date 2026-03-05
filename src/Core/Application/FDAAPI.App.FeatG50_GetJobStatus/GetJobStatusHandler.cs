using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG50_GetJobStatus
{
    public class GetJobStatusHandler : IRequestHandler<GetJobStatusRequest, GetJobStatusResponse>
    {
        private readonly IAnalyticsJobRunRepository _jobRunRepository;

        public GetJobStatusHandler(IAnalyticsJobRunRepository jobRunRepository)
        {
            _jobRunRepository = jobRunRepository;
        }

        public async Task<GetJobStatusResponse> Handle(
            GetJobStatusRequest request,
            CancellationToken ct)
        {
            try
            {
                var jobRun = await _jobRunRepository.GetByIdAsync(request.JobRunId, ct);

                if (jobRun == null)
                {
                    return new GetJobStatusResponse
                    {
                        Success = false,
                        Message = "Job run not found",
                        StatusCode = AnalyticsStatusCode.NotFound
                    };
                }

                return new GetJobStatusResponse
                {
                    Success = true,
                    Message = "Job status retrieved successfully",
                    StatusCode = AnalyticsStatusCode.Success,
                    Data = new JobStatusDto
                    {
                        JobRunId = jobRun.Id,
                        JobType = jobRun.Job?.JobType ?? "UNKNOWN",
                        Status = jobRun.Status,
                        StartedAt = jobRun.StartedAt,
                        FinishedAt = jobRun.FinishedAt,
                        ExecutionTimeMs = jobRun.ExecutionTimeMs,
                        RecordsProcessed = jobRun.RecordsProcessed,
                        RecordsCreated = jobRun.RecordsCreated,
                        ErrorMessage = jobRun.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetJobStatusResponse
                {
                    Success = false,
                    Message = $"Error retrieving job status: {ex.Message}",
                    StatusCode = AnalyticsStatusCode.InternalServerError
                };
            }
        }
    }
}

