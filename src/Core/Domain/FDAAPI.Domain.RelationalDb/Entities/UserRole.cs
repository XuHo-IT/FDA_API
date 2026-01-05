using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class UserRole : EntityWithId<Guid>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;
    }
}






