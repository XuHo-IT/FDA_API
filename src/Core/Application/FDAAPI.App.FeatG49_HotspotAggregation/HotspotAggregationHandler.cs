using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Hangfire;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public class HotspotAggregationHandler : IRequestHandler<HotspotAggregationRequest, HotspotAggregationResponse>
    {
        private readonly IAnalyticsJobRepository _jobRepository;
        private readonly IAnalyticsJobRunRepository _jobRunRepository;
        private readonly HotspotAggregationBackgroundJob _backgroundJob;

        public HotspotAggregationHandler(
            IAnalyticsJobRepository jobRepository,
            IAnalyticsJobRunRepository jobRunRepository,
            HotspotAggregationBackgroundJob backgroundJob)
        {
            _jobRepository = jobRepository;
            _jobRunRepository = jobRunRepository;
            _backgroundJob = backgroundJob;
        }

        public async Task<HotspotAggregationResponse> Handle(
            HotspotAggregationRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get or create analytics job
                var job = await _jobRepository.GetByJobTypeAsync("HOTSPOT_AGG", ct);
                if (job == null)
                {
                    job = new AnalyticsJob
                    {
                        Id = Guid.NewGuid(),
                        JobType = "HOTSPOT_AGG",
                        Schedule = "Weekly on Monday",
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
                    request.PeriodStart,
                    request.PeriodEnd,
                    request.TopN,
                    jobRunId));

                return new HotspotAggregationResponse
                {
                    Success = true,
                    Message = "Hotspot aggregation job started",
                    StatusCode = AnalyticsStatusCode.Accepted,
                    Data = new JobRunDto
                    {
                        JobRunId = jobRunId,
                        JobType = "HOTSPOT_AGG",
                        Status = "RUNNING",
                        StartedAt = jobRun.StartedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new HotspotAggregationResponse
                {
                    Success = false,
                    Message = $"Error starting aggregation job: {ex.Message}",
                    StatusCode = AnalyticsStatusCode.InternalServerError
                };
            }
        }

    }
}

