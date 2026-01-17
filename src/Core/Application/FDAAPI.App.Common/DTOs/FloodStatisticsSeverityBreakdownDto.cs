using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Breakdown of hours by severity level
    /// </summary>
    public class FloodStatisticsSeverityBreakdownDto
    {
        public int HoursSafe { get; set; }
        public int HoursCaution { get; set; }
        public int HoursWarning { get; set; }
        public int HoursCritical { get; set; }
    }
}
