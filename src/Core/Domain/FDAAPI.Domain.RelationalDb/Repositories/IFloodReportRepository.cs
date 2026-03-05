using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodReportRepository
    {
        Task<FloodReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(FloodReport entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(FloodReport entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        
        // Find nearby published reports for consensus calculation
        Task<List<FloodReport>> FindNearbyPublishedReportsAsync(
            decimal latitude,
            decimal longitude,
            int radiusMeters,
            TimeSpan timeWindow,
            CancellationToken ct = default);
        
        // Get reports by status
        Task<List<FloodReport>> GetByStatusAsync(string status, CancellationToken ct = default);
        
        // Get reports by reporter
        Task<List<FloodReport>> GetByReporterIdAsync(Guid reporterId, CancellationToken ct = default);

        // Paged list with basic filters (for FeatG83)
        Task<(IReadOnlyList<FloodReport> Items, int TotalCount)> ListAsync(
            string? status,
            string? severity,
            DateTime? from,
            DateTime? to,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
    }
}

