using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class OtpCode : EntityWithId<Guid>, ICreatedEntity<Guid>
    {
        [Obsolete("Use Identifier instead")]
        public string PhoneNumber { get; set; } = string.Empty; // Keep for backward compatibility

        /// <summary>
        /// Phone number OR email address (identifier)
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Type of identifier: "phone" or "email"
        /// </summary>
        public string IdentifierType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // 6-digit
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
