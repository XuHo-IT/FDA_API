using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class AlertSummaryDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> BySeverity { get; set; } = new();
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }
}
