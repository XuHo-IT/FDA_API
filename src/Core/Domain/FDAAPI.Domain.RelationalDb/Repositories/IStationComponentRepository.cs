using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for StationComponent entity
    /// </summary>
    public interface IStationComponentRepository
    {
        /// <summary>
        /// Get all components for a station
        /// </summary>
        Task<IEnumerable<StationComponent>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get component by ID
        /// </summary>
        Task<StationComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a component type already exists in a station
        /// </summary>
        Task<bool> ExistsByTypeAsync(Guid stationId, string componentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new component
        /// </summary>
        Task<StationComponent> CreateAsync(StationComponent component, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing component
        /// </summary>
        Task<StationComponent> UpdateAsync(StationComponent component, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a component
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
