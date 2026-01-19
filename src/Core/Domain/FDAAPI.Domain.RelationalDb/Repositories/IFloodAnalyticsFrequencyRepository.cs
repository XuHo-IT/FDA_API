using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodAnalyticsFrequencyRepository
    {
        Task<List<FloodAnalyticsFrequency>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            string bucketType,
            CancellationToken ct = default);

        Task BulkUpsertAsync(
            List<FloodAnalyticsFrequency> aggregates,
            CancellationToken ct = default);

        Task<FloodAnalyticsFrequency?> GetByAdministrativeAreaBucketAsync(
            Guid administrativeAreaId,
            DateTime timeBucket,
            string bucketType,
            CancellationToken ct = default);
    }
}

