using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class HotspotDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public string AdministrativeAreaName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int Rank { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
