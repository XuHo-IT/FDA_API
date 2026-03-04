using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IFloodReportMediaRepository
    {
        Task<FloodReportMedia?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(FloodReportMedia entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(FloodReportMedia entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        
        // Get all media for a report
        Task<List<FloodReportMedia>> GetByReportIdAsync(Guid reportId, CancellationToken ct = default);
    }
}

