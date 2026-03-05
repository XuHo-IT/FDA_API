using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface ISensorDailyAggRepository
    {
        Task<List<SensorDailyAgg>> GetByStationAndRangeAsync(
            Guid stationId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);

        Task<List<SensorDailyAgg>> GetByStationsAndRangeAsync(
            List<Guid> stationIds,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);

        Task<SensorDailyAgg?> GetByStationAndDateAsync(
            Guid stationId,
            DateOnly date,
            CancellationToken ct = default);

        Task<Guid> CreateAsync(SensorDailyAgg aggregate, CancellationToken ct = default);

        Task BulkInsertAsync(IEnumerable<SensorDailyAgg> aggregates, CancellationToken ct = default);

        /// <summary>
        /// Get statistics summary for a station over a date range
        /// </summary>
        Task<(decimal MaxLevel, decimal MinLevel, decimal AvgLevel, int TotalFloodHours, int TotalReadings)>
            GetStatisticsSummaryAsync(
                Guid stationId,
                DateOnly startDate,
                DateOnly endDate,
                CancellationToken ct = default);
    }
}
