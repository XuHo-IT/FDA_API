// File: FDA_API/src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/IAlertTemplateRepository.cs

using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAlertTemplateRepository
    {
        Task<AlertTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(AlertTemplate entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(AlertTemplate entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

        // Get all templates with optional filters
        Task<IEnumerable<AlertTemplate>> GetAllAsync(
            bool? isActive = null,
            string? channel = null,
            string? severity = null,
            CancellationToken ct = default);

        // Get template by channel and severity (for notification dispatch)
        Task<AlertTemplate?> GetByChannelAndSeverityAsync(
            string channel,
            string? severity,
            CancellationToken ct = default);

        // Get template by channel only (fallback)
        Task<AlertTemplate?> GetByChannelAsync(
            string channel,
            CancellationToken ct = default);
    }
}
