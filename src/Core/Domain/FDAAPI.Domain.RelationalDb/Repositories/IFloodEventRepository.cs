using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodEventRepository
    {
        Task<FloodEvent?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<int> CountByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<List<FloodEvent>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<Guid> CreateAsync(
            FloodEvent floodEvent,
            CancellationToken ct = default);

        Task<bool> UpdateAsync(
            FloodEvent floodEvent,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);

        Task<(IEnumerable<FloodEvent> Events, int TotalCount)> GetFloodEventsAsync(
            string? searchTerm,
            Guid? administrativeAreaId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<List<FloodEvent>> GetActiveFloodEventsAsync(CancellationToken ct = default);
    }
}

