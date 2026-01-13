using FDAAPI.Domain.RelationalDb.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class Station : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Code { get; set; } 
        public string Name { get; set; }
        public string LocationDesc { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string RoadName { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
        public decimal? ThresholdWarning { get; set; }
        public decimal? ThresholdCritical { get; set; }
        public DateTimeOffset? InstalledAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
