using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface ISensorHourlyAggRepository
    {
        Task<List<SensorHourlyAgg>> GetByStationAndRangeAsync(
            Guid stationId,
            DateTime startHour,
            DateTime endHour,
            CancellationToken ct = default);

        Task<List<SensorHourlyAgg>> GetByStationsAndRangeAsync(
            List<Guid> stationIds,
            DateTime startHour,
            DateTime endHour,
            CancellationToken ct = default);

        Task<SensorHourlyAgg?> GetByStationAndHourAsync(
            Guid stationId,
            DateTime hourStart,
            CancellationToken ct = default);

        Task<Guid> CreateAsync(SensorHourlyAgg aggregate, CancellationToken ct = default);

        Task BulkInsertAsync(IEnumerable<SensorHourlyAgg> aggregates, CancellationToken ct = default);
    }
}
