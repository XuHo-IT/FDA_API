using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class RefreshToken : EntityWithId<Guid>, ICreatedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}






