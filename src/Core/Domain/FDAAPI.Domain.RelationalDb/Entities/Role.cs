using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class Role : EntityWithId<Guid>
    {
        public string Code { get; set; } = string.Empty; // e.g., ADMIN (SPUER_ADMIN), USER, MORDERATOR
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}






