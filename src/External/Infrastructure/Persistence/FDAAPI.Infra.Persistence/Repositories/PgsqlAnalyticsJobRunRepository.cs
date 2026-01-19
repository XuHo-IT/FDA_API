using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAnalyticsJobRunRepository : IAnalyticsJobRunRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAnalyticsJobRunRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(
            AnalyticsJobRun jobRun,
            CancellationToken ct = default)
        {
            _context.AnalyticsJobRuns.Add(jobRun);
            await _context.SaveChangesAsync(ct);
            return jobRun.Id;
        }

        public async Task<AnalyticsJobRun?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            return await _context.AnalyticsJobRuns
                .AsNoTracking()
                .Include(jr => jr.Job)
                .FirstOrDefaultAsync(jr => jr.Id == id, ct);
        }

        public async Task<bool> UpdateStatusAsync(
            Guid id,
            string status,
            string? errorMessage,
            DateTime? finishedAt,
            int? executionTimeMs,
            int? recordsProcessed,
            int? recordsCreated,
            CancellationToken ct = default)
        {
            var jobRun = await _context.AnalyticsJobRuns
                .FirstOrDefaultAsync(jr => jr.Id == id, ct);

            if (jobRun == null)
                return false;

            jobRun.Status = status;
            jobRun.ErrorMessage = errorMessage;
            jobRun.FinishedAt = finishedAt;
            jobRun.ExecutionTimeMs = executionTimeMs;
            jobRun.RecordsProcessed = recordsProcessed ?? jobRun.RecordsProcessed;
            jobRun.RecordsCreated = recordsCreated ?? jobRun.RecordsCreated;

            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }
    }
}

