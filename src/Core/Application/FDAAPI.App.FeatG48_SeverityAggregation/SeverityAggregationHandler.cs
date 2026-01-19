using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Hangfire;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG48_SeverityAggregation
{
    public class SeverityAggregationHandler : IRequestHandler<SeverityAggregationRequest, SeverityAggregationResponse>
    {
        private readonly IAnalyticsJobRepository _jobRepository;
        private readonly IAnalyticsJobRunRepository _jobRunRepository;
        private readonly SeverityAggregationBackgroundJob _backgroundJob;

        public SeverityAggregationHandler(
            IAnalyticsJobRepository jobRepository,
            IAnalyticsJobRunRepository jobRunRepository,
            SeverityAggregationBackgroundJob backgroundJob)
        {
            _jobRepository = jobRepository;
            _jobRunRepository = jobRunRepository;
            _backgroundJob = backgroundJob;
        }

        public async Task<SeverityAggregationResponse> Handle(
            SeverityAggregationRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get or create analytics job
                var job = await _jobRepository.GetByJobTypeAsync("SEVERITY_AGG", ct);
                if (job == null)
                {
                    job = new AnalyticsJob
                    {
                        Id = Guid.NewGuid(),
                        JobType = "SEVERITY_AGG",
                        Schedule = "Daily at 2 AM",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _jobRepository.CreateAsync(job, ct);
                }

                // 2. Create job run
                var jobRun = new AnalyticsJobRun
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    StartedAt = DateTime.UtcNow,
                    Status = "RUNNING",
                    RecordsProcessed = 0,
                    RecordsCreated = 0,
                    CreatedAt = DateTime.UtcNow
                };
                var jobRunId = await _jobRunRepository.CreateAsync(jobRun, ct);

                // 3. Enqueue aggregation job in Hangfire
                // Note: CancellationToken removed from ExecuteAsync signature because Hangfire cannot serialize it
                BackgroundJob.Enqueue(() => _backgroundJob.ExecuteAsync(
                    request.BucketType,
                    request.StartDate,
                    request.EndDate,
                    request.AdministrativeAreaIds,
                    jobRunId));

                return new SeverityAggregationResponse
                {
                    Success = true,
                    Message = "Severity aggregation job started",
                    StatusCode = AnalyticsStatusCode.Accepted,
                    Data = new JobRunDto
                    {
                        JobRunId = jobRunId,
                        JobType = "SEVERITY_AGG",
                        Status = "RUNNING",
                        StartedAt = jobRun.StartedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new SeverityAggregationResponse
                {
                    Success = false,
                    Message = $"Error starting aggregation job: {ex.Message}",
                    StatusCode = AnalyticsStatusCode.InternalServerError
                };
            }
        }

    }
}

