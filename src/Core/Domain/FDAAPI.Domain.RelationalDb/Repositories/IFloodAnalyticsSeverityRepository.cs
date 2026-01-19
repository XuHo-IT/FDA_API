using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodAnalyticsSeverityRepository
    {
        Task<List<FloodAnalyticsSeverity>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            string bucketType,
            CancellationToken ct = default);

        Task BulkUpsertAsync(
            List<FloodAnalyticsSeverity> aggregates,
            CancellationToken ct = default);

        Task<FloodAnalyticsSeverity?> GetByAdministrativeAreaBucketAsync(
            Guid administrativeAreaId,
            DateTime timeBucket,
            string bucketType,
            CancellationToken ct = default);
    }
}

