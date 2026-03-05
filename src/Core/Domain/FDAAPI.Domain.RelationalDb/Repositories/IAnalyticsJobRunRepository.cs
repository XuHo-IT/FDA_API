using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAnalyticsJobRunRepository
    {
        Task<Guid> CreateAsync(
            AnalyticsJobRun jobRun,
            CancellationToken ct = default);

        Task<AnalyticsJobRun?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<bool> UpdateStatusAsync(
            Guid id,
            string status,
            string? errorMessage,
            DateTime? finishedAt,
            int? executionTimeMs,
            int? recordsProcessed,
            int? recordsCreated,
            CancellationToken ct = default);
    }
}

