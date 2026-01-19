using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodAnalyticsHotspotRepository
    {
        Task<List<FloodAnalyticsHotspot>> GetTopHotspotsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            int topN,
            CancellationToken ct = default);

        Task BulkUpsertAsync(
            List<FloodAnalyticsHotspot> hotspots,
            CancellationToken ct = default);
    }
}

