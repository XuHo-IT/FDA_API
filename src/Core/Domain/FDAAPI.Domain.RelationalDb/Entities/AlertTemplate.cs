// File: FDA_API/src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/AlertTemplate.cs

using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Alert notification template - allows customizable message templates per channel/severity
    /// </summary>
    public class AlertTemplate : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        /// <summary>
        /// Template name for admin reference
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Notification channel: Push, Email, SMS, InApp
        /// </summary>
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Alert severity level this template applies to. Null = all severities
        /// </summary>
        public string? Severity { get; set; }

        /// <summary>
        /// Title template with variable placeholders
        /// </summary>
        public string TitleTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Body template with variable placeholders
        /// </summary>
        public string BodyTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Is template active for use
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Display order in admin UI
        /// </summary>
        public int SortOrder { get; set; }

        // Audit fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
