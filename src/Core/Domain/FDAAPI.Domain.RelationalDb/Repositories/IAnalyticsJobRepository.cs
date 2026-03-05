using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAnalyticsJobRepository
    {
        Task<AnalyticsJob?> GetByJobTypeAsync(
            string jobType,
            CancellationToken ct = default);

        Task<Guid> CreateAsync(
            AnalyticsJob job,
            CancellationToken ct = default);

        Task<bool> UpdateAsync(
            AnalyticsJob job,
            CancellationToken ct = default);
    }
}

