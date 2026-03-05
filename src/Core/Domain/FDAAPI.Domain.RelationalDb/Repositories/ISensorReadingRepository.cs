using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface ISensorReadingRepository
    {
        Task<SensorReading?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(SensorReading entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(SensorReading entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<SensorReading>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);

        Task<(IEnumerable<SensorReading> Readings, int TotalCount)> GetSensorReadingsAsync(
            Guid? stationId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<List<SensorReading>> GetLatestReadingsByStationsAsync(
            IEnumerable<Guid> stationIds,
            CancellationToken ct = default);

        Task<List<SensorReading>> GetByStationAndTimeRangeAsync(
            Guid stationId,
            DateTime startTime,
            DateTime endTime,
            int limit,
            CancellationToken ct = default);

        Task<List<SensorReading>> GetByHourAsync(
            DateTime hourStart,
            CancellationToken ct = default);

        Task<int> CountExceedancesByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<List<SensorReading>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<List<(SensorReading Reading, Station Station)>> GetReadingsWithStationsByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);
    }
}
